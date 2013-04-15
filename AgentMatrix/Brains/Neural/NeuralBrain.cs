using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Brains;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Entities;
using DawnOnline.Simulation.Senses;
using DawnOnline.Simulation.Tools;
using Microsoft.Xna.Framework;
using SharedConstants;

namespace DawnOnline.AgentMatrix.Brains.Neural
{
    internal class NeuralBrain : AbstractBrain
    {
        private IEye _forwardEye;
        private IEye _leftEye;
        private IEye _rightEye;

        private Dictionary<IEye, double> _eyeSeeEnemy = new Dictionary<IEye, double>();
        private Dictionary<IEye, double> _eyeSeeTreasure = new Dictionary<IEye, double>();
        private Dictionary<IEye, double> _eyeSeeWalls = new Dictionary<IEye, double>();
        private Dictionary<IEye, double> _eyeSeeMySpawnPoint = new Dictionary<IEye, double>();

        private IBumper _forwardBumper;
        private bool _initialized;

        private IEye _nose;
        private Dictionary<IEye, double> _smellMySpawnPoint = new Dictionary<IEye, double>();


        private const int _modeInputNodes = 19; // 3x eye x4, bumper, nose, 2x random, health, stamina, resources
        private const int _modeOutputNodes = 2; // Turn, Trust
        private NeuralNetwork _adrenalineModeNetwork;
        private NeuralNetwork _foragerModeNetwork;
        private NeuralNetwork _deliverModeNetwork;

        private NeuralNetwork _modeChoserNetwork;
        private const int _modeChoserInputNodes = 19; // 3x eye x4, bumper, nose, 2x random, health, stamina, resources
        private const int _modeChoserOutputNodes = 3; // The modes

        public override void DoSomething(TimeSpan timeDelta)
        {
            Debug.Assert(MyCreature != null);
            Debug.Assert(_initialized);

            // Fill eye buffer
            SeeEnemies();
            SeeTreasures();
            SeeWalls();
            SeeAndSmellSpawnPoint();


            var modeNetwork = ChoseCurrentMode();

            ClearReinforcementOtherModeNetworks(modeNetwork);
            RunNetwork(modeNetwork);


            //// I have resources => deliver
            //if (MyCreature.CharacterSheet.Resource.PercentFilled > 5)
            //{
            //    DeliverState(timeDelta);
            //    return;
            //}

            //if (ISeeAnEnemy())
            //{
            //    // CHAAAAARGE!
            //    AdrenalineState(timeDelta);
            //    return;
            //}

            //ForageState(timeDelta);
        }

        private bool ISeeAnEnemy()
        {
            var check = _eyeSeeEnemy[_forwardEye] > 0 || _eyeSeeEnemy[_leftEye] > 0 || _eyeSeeEnemy[_rightEye] > 0;
            // get a good look!
            //var check = _eyeSeeEnemy[_forwardEye] > 5 || _eyeSeeEnemy[_leftEye] > 5 || _eyeSeeEnemy[_rightEye] > 5;

            return check;
        }

        private void SeeEnemies()
        {
            var entities = MyCreature.MyEnvironment.GetCreatures(MyCreature.FoodSpecies);
            var sortedOnDistance = FilterAndSortOnDistance(entities, MyCreature.CharacterSheet.VisionDistance);

            // Filter
            var filtered = new List<IEntity>();
            foreach (ICreature entity in sortedOnDistance)
            {
                // Not my family
                if (entity.SpawnPoint == MyCreature.SpawnPoint)
                    continue;

                filtered.Add(entity);
            }

            _eyeSeeEnemy.Clear();

            _eyeSeeEnemy.Add(_forwardEye, _forwardEye.WeightedDistanceToFirstVisible(filtered));
            _eyeSeeEnemy.Add(_leftEye, _leftEye.WeightedDistanceToFirstVisible(filtered));
            _eyeSeeEnemy.Add(_rightEye, _rightEye.WeightedDistanceToFirstVisible(filtered));
        }

        private void SeeTreasures()
        {
            var entities = MyCreature.MyEnvironment.GetObstacles().Where(e => e.Specy == EntityType.Treasure);
            var sortedOnDistance = FilterAndSortOnDistance(entities, MyCreature.CharacterSheet.VisionDistance);

            _eyeSeeTreasure.Clear();

            _eyeSeeTreasure.Add(_forwardEye, _forwardEye.WeightedDistanceToFirstVisible(sortedOnDistance));
            _eyeSeeTreasure.Add(_leftEye, _leftEye.WeightedDistanceToFirstVisible(sortedOnDistance));
            _eyeSeeTreasure.Add(_rightEye, _rightEye.WeightedDistanceToFirstVisible(sortedOnDistance));
        }

        private void SeeWalls()
        {
            var entities = MyCreature.MyEnvironment.GetObstacles().Where(e => e.Specy == EntityType.Wall || e.Specy == EntityType.Box);
            var sortedOnDistance = FilterAndSortOnDistance(entities, MyCreature.CharacterSheet.VisionDistance);

            _eyeSeeWalls.Clear();

            _eyeSeeWalls.Add(_forwardEye, _forwardEye.WeightedDistanceToFirstVisible(sortedOnDistance, false));
            _eyeSeeWalls.Add(_leftEye, _leftEye.WeightedDistanceToFirstVisible(sortedOnDistance, false));
            _eyeSeeWalls.Add(_rightEye, _rightEye.WeightedDistanceToFirstVisible(sortedOnDistance, false));
        }

        private void SeeAndSmellSpawnPoint()
        {
            var entities = MyCreature.MyEnvironment.GetCreatures().Where(e => e.Specy == EntityType.SpawnPoint);
            var sortedOnDistance = FilterAndSortOnDistance(entities, 1000);

            // Filter
            var filtered = new List<IEntity>();
            foreach (ICreature entity in sortedOnDistance)
            {
                // Not my enemy
                if (entity.SpawnPoint != MyCreature.SpawnPoint)
                    continue;

                filtered.Add(entity);
            }

            // See
            _eyeSeeMySpawnPoint.Clear();

            _eyeSeeMySpawnPoint.Add(_forwardEye, _forwardEye.WeightedDistanceToFirstVisible(filtered));
            _eyeSeeMySpawnPoint.Add(_leftEye, _leftEye.WeightedDistanceToFirstVisible(filtered));
            _eyeSeeMySpawnPoint.Add(_rightEye, _rightEye.WeightedDistanceToFirstVisible(filtered));

            // Smell
            _smellMySpawnPoint.Clear();

            _smellMySpawnPoint.Add(_nose, _nose.WeightedDistanceToFirstVisible(filtered, false));
        }

        public override void InitializeSenses()
        {
            // Eyes
            _forwardEye = SensorBuilder.CreateEye(MyCreature, 0.0, MathTools.ConvertToRadials(30), MyCreature.CharacterSheet.VisionDistance);
            _leftEye = SensorBuilder.CreateEye(MyCreature, -MathTools.ConvertToRadials(60), MathTools.ConvertToRadials(60), MyCreature.CharacterSheet.VisionDistance);
            _rightEye = SensorBuilder.CreateEye(MyCreature, MathTools.ConvertToRadials(60), MathTools.ConvertToRadials(60), MyCreature.CharacterSheet.VisionDistance);

            // Bumpers
            _forwardBumper = SensorBuilder.CreateBumper(MyCreature, new Vector2((float)MyCreature.Place.Form.BoundingCircleRadius, 0));

            // Ears
            //_leftEar = SensorBuilder.CreateEar(MyCreature, new Vector2(0, -2));
            //_rightEar = SensorBuilder.CreateEar(MyCreature, new Vector2(0, -2));

            // Nose
            // TODO: Connect to CharacterSheet
            _nose = SensorBuilder.CreateEye(MyCreature, 0.0, MathTools.ConvertToRadials(90), 200);

            _initialized = true;
        }

        private static NeuralNetwork PredefineModeChoser(int inputNodes, int outputNodes)
        {
            // OutputNodes:
            // > ForagerMode
            // > DeliverMode
            // > AdrenalineMode

            var network = new NeuralNetwork(inputNodes, inputNodes * 2, outputNodes + inputNodes, inputNodes);

            // Monitors
            // > Damage
            //network.InputNodes[16].OutGoingEdges[1].Initialize(1);
            //network.InputNodes[16].OutGoingEdges[2].Initialize(-.5);
            // > Fatigue
            // > Resources
            //network.InputNodes[18].OutGoingEdges[0].Initialize(-.5);
            network.InputNodes[18].OutGoingEdges[1].Initialize(10);
            //network.InputNodes[18].OutGoingEdges[2].Initialize(-1);


            network.LayerNodes[0].OutGoingEdges[0].Initialize(1); // ForagerMode
            network.LayerNodes[1].OutGoingEdges[1].Initialize(1); // DeliverMode
            network.LayerNodes[2].OutGoingEdges[2].Initialize(1); // AdrenalineMode


            // > ISeeEnemy
            network.ReinforcementInputNodes[0].OutGoingEdges[2].Initialize(.3);
            network.InputNodes[1].OutGoingEdges[2].Initialize(1);
            network.ReinforcementInputNodes[2].OutGoingEdges[2].Initialize(.3);
            // > ISeeTreasure
            network.InputNodes[4].OutGoingEdges[0].Initialize(1);
            // > ISeeMySpawnPoint
            network.InputNodes[10].OutGoingEdges[1].Initialize(1);



            /////////////////////////////////////////////
            // Reinforcement
            BuildStandardReinforcementSetup(network);

            // > ISeeEnemy
            //network.ReinforcementInputNodes[0].OutGoingEdges[2].Initialize(.3);
            //network.ReinforcementInputNodes[1].OutGoingEdges[2].Initialize(1);
            //network.ReinforcementInputNodes[2].OutGoingEdges[2].Initialize(.3);
            // > ISeeTreasure
            //network.ReinforcementInputNodes[4].OutGoingEdges[0].Initialize(1);
            // > ISeeMySpawnPoint
            //network.ReinforcementInputNodes[10].OutGoingEdges[1].Initialize(1);


            // Random to enforce default = ForageMode
            network.ReinforcementInputNodes[14].OutGoingEdges[0].Initialize(.1);


            return network;
        }

        private static NeuralNetwork PredefineAdrenalineMode(int inputNodes, int outputNodes)
        {
            var network = new NeuralNetwork(inputNodes, inputNodes * 2, outputNodes + inputNodes, inputNodes);

            // Eyes
            // > ISeeEnemy
            network.InputNodes[0].OutGoingEdges[0].Initialize(1.5);
            network.InputNodes[1].OutGoingEdges[1].Initialize(1.5);
            network.InputNodes[2].OutGoingEdges[2].Initialize(1.5);

            network.LayerNodes[0].OutGoingEdges[0].Initialize(-1.5);
            network.LayerNodes[1].OutGoingEdges[1].Initialize(1.5);
            network.LayerNodes[2].OutGoingEdges[0].Initialize(1.5);

            // bumper


            // Reinforcement
            BuildStandardReinforcementSetup(network);

            network.ReinforcementInputNodes[0].OutGoingEdges[0].Initialize(0.5);
            network.ReinforcementInputNodes[1].OutGoingEdges[1].Initialize(0.5);
            network.ReinforcementInputNodes[2].OutGoingEdges[2].Initialize(0.5);

            return network;
        }

        private static NeuralNetwork PredefineForagerMode(int inputNodes, int outputNodes)
        {
            var network = new NeuralNetwork(inputNodes, inputNodes*2, outputNodes + inputNodes, inputNodes);

            // eyes
            // > ISeeTreasure
            network.InputNodes[3].OutGoingEdges[0].Initialize(1);
            network.InputNodes[4].OutGoingEdges[1].Initialize(1);
            network.InputNodes[5].OutGoingEdges[2].Initialize(1);

            network.LayerNodes[0].OutGoingEdges[0].Initialize(-1);
            network.LayerNodes[1].OutGoingEdges[1].Initialize(1);
            network.LayerNodes[2].OutGoingEdges[0].Initialize(1);


            // Reinforcement
            BuildStandardReinforcementSetup(network);


            // bumper
            network.ReinforcementInputNodes[12].OutGoingEdges[0].Initialize(0.5);
            network.ReinforcementInputNodes[12].OutGoingEdges[1].Initialize(-2);
            network.ReinforcementInputNodes[12].OutGoingEdges[2].Initialize(-0.5);

            // random nodes

            // reinforced random: input nodes
            network.ReinforcementInputNodes[14].OutGoingEdges[0].Initialize(.1);
            network.ReinforcementInputNodes[14].OutGoingEdges[1].Initialize(.3);
            network.ReinforcementInputNodes[14].OutGoingEdges[2].Initialize(-.1);

            network.ReinforcementInputNodes[15].OutGoingEdges[0].Initialize(-.1);
            network.ReinforcementInputNodes[15].OutGoingEdges[1].Initialize(.3);
            network.ReinforcementInputNodes[15].OutGoingEdges[2].Initialize(.1);

            return network;
        }

        private static NeuralNetwork PredefineDeliverMode(int inputNodes, int outputNodes)
        {
            var network = new NeuralNetwork(inputNodes, inputNodes*2, outputNodes + inputNodes, inputNodes);

            // eyes
            // > ISeeMySpawnPoint
            network.InputNodes[9].OutGoingEdges[0].Initialize(1);
            network.InputNodes[10].OutGoingEdges[1].Initialize(1);
            network.InputNodes[11].OutGoingEdges[2].Initialize(1);

            network.LayerNodes[0].OutGoingEdges[0].Initialize(-2);
            network.LayerNodes[1].OutGoingEdges[1].Initialize(2);
            network.LayerNodes[2].OutGoingEdges[0].Initialize(2);


            // Reinforcement
            BuildStandardReinforcementSetup(network);


            // bumper
            network.ReinforcementInputNodes[12].OutGoingEdges[0].Initialize(0.5);
            network.ReinforcementInputNodes[12].OutGoingEdges[1].Initialize(-2);
            network.ReinforcementInputNodes[12].OutGoingEdges[2].Initialize(-0.5);


            // Smells like home
            network.InputNodes[13].OutGoingEdges[1].Initialize(5);

            // random nodes

            // reinforced random: input nodes
            network.ReinforcementInputNodes[14].OutGoingEdges[0].Initialize(.1);
            network.ReinforcementInputNodes[14].OutGoingEdges[1].Initialize(.1); 
            
            return network;
        }

        internal void PredefineBehaviour()
        {
            // Masterbrain
            _modeChoserNetwork = PredefineModeChoser(_modeChoserInputNodes, _modeChoserOutputNodes);

            // AdrenalineMode
            _adrenalineModeNetwork = PredefineAdrenalineMode(_modeInputNodes, _modeOutputNodes);

            // ForagerMode
            _foragerModeNetwork = PredefineForagerMode(_modeInputNodes, _modeOutputNodes);

            // DeliverMode
            _deliverModeNetwork = PredefineDeliverMode(_modeInputNodes, _modeOutputNodes);

            // Memory
            ConnectReinforcementNodes();
        }

        private static void BuildStandardReinforcementSetup(NeuralNetwork network)
        {
            Debug.Assert(network.InputNodes.Length == network.ReinforcementInputNodes.Length);

            // input to reinforcement layer nodes
            const double originalDecay = 1;
            for (int i = 0; i < network.InputNodes.Length; i++)
            {
                network.InputNodes[i].OutGoingEdges[network.InputNodes.Length + i].Initialize(originalDecay);
            }
            // standard reinforcement setup: small decay
            const double decay = 0.9;
            var nrOfOutputNodes = network.OutputNodes.Length - network.ReinforcementInputNodes.Length;
            for (int i = 0; i < network.ReinforcementInputNodes.Length; i++)
            {
                network.ReinforcementInputNodes[i].OutGoingEdges[network.InputNodes.Length + i].Initialize(decay);
                network.LayerNodes[network.InputNodes.Length + i].OutGoingEdges[nrOfOutputNodes + i].Initialize(decay);
            }
        }

        private void ConnectReinforcementNodes()
        {
            // WTF doet dit nu weer???

            for (int i = 0; i < _modeChoserInputNodes; i++)
            {
                _modeChoserNetwork.ReinforcementInputNodes[i].NodeToReinforce = (_modeChoserNetwork.OutputNodes[_modeChoserOutputNodes + i]);
            }

            for (int i = 0; i < _modeInputNodes; i++)
            {
                _adrenalineModeNetwork.ReinforcementInputNodes[i].NodeToReinforce = (_adrenalineModeNetwork.OutputNodes[_modeOutputNodes+i]);
            }
            for (int i = 0; i < _modeInputNodes; i++)
            {
                _foragerModeNetwork.ReinforcementInputNodes[i].NodeToReinforce = (_foragerModeNetwork.OutputNodes[_modeOutputNodes+i]);
            }
            for (int i = 0; i < _modeInputNodes; i++)
            {
                _deliverModeNetwork.ReinforcementInputNodes[i].NodeToReinforce = (_deliverModeNetwork.OutputNodes[_modeOutputNodes + i]);
            }
        }

        public override void ClearState()
        {
            _forwardBumper.Clear();

            _adrenalineModeNetwork.ClearInput();
            _foragerModeNetwork.ClearInput();
            _deliverModeNetwork.ClearInput();
            _modeChoserNetwork.ClearInput();
        }

        private void FillInputNodes(NeuralNetwork network)
        {
            int i = 0;
            if (_eyeSeeEnemy.Count > 0)
            {
                network.InputNodes[i++].CurrentValue = _eyeSeeEnemy[_leftEye];
                network.InputNodes[i++].CurrentValue = _eyeSeeEnemy[_forwardEye];
                network.InputNodes[i++].CurrentValue = _eyeSeeEnemy[_rightEye];
            }
            else i += 3;
            if (_eyeSeeTreasure.Count > 0)
            {
                network.InputNodes[i++].CurrentValue = _eyeSeeTreasure[_leftEye];
                network.InputNodes[i++].CurrentValue = _eyeSeeTreasure[_forwardEye];
                network.InputNodes[i++].CurrentValue = _eyeSeeTreasure[_rightEye];
            }
            else i += 3;
            if (_eyeSeeWalls.Count > 0)
            {
                network.InputNodes[i++].CurrentValue = _eyeSeeWalls[_leftEye];
                network.InputNodes[i++].CurrentValue = _eyeSeeWalls[_forwardEye];
                network.InputNodes[i++].CurrentValue = _eyeSeeWalls[_rightEye];
            }
            else i += 3;
            if (_eyeSeeMySpawnPoint.Count > 0)
            {
                network.InputNodes[i++].CurrentValue = _eyeSeeMySpawnPoint[_leftEye];
                network.InputNodes[i++].CurrentValue = _eyeSeeMySpawnPoint[_forwardEye];
                network.InputNodes[i++].CurrentValue = _eyeSeeMySpawnPoint[_rightEye];
            }
            else i += 3;
            Debug.Assert(i == 12);
            network.InputNodes[i++].CurrentValue = _forwardBumper.Hit ? 100 : 0;
            network.InputNodes[i++].CurrentValue = _smellMySpawnPoint[_nose];
            network.InputNodes[i++].CurrentValue = Globals.Radomizer.Next(50);
            network.InputNodes[i++].CurrentValue = Globals.Radomizer.Next(50);
            Debug.Assert(i == 16);
            network.InputNodes[i++].CurrentValue = this.MyCreature.CharacterSheet.Damage.PercentFilled;
            network.InputNodes[i++].CurrentValue = this.MyCreature.CharacterSheet.Fatigue.PercentFilled;
            network.InputNodes[i++].CurrentValue = this.MyCreature.CharacterSheet.Resource.PercentFilled;
            Debug.Assert(i == 19);
        }

        private void RunNetwork(NeuralNetwork network)
        {
            FillInputNodes(network);

            // Process
            network.Propagate();

            // Feed output to creature
            MyCreature.Turn(network.OutputNodes[0].CurrentValue / 100);
            MyCreature.Thrust(network.OutputNodes[1].CurrentValue / 100);

            // Speak
            //MyCreature.SayA(network.OutputNodes[2].CurrentValue);
            //MyCreature.SayB(network.OutputNodes[3].CurrentValue);

            // Attack when possible
            // TODO: SHOULD BE OUTPUT OF NETWORK?
            // TODO => currently replacd by AUTOATTACK PROPERTY
            //if (MyCreature.FindCreatureToAttack(MyCreature.FoodSpecies) != null)
            //{
            //    MyCreature.Attack();
            //}
        }

        private NeuralNetwork ChoseCurrentMode()
        {
            FillInputNodes(_modeChoserNetwork);

            // Process
            _modeChoserNetwork.Propagate();

            var forageWeight = _modeChoserNetwork.OutputNodes[0].CurrentValue;
            var deliverWeight = _modeChoserNetwork.OutputNodes[1].CurrentValue;
            var adrenalineWeight = _modeChoserNetwork.OutputNodes[2].CurrentValue;

            if ((forageWeight >= deliverWeight) && (forageWeight >= adrenalineWeight)) return _foragerModeNetwork;
            if ((deliverWeight >= forageWeight) && (deliverWeight >= adrenalineWeight)) return _deliverModeNetwork;
            Debug.Assert((adrenalineWeight >= forageWeight) && (adrenalineWeight >= deliverWeight));
            return _adrenalineModeNetwork;
        }

        private void ClearReinforcementOtherModeNetworks(NeuralNetwork network)
        {
            if ( network != _adrenalineModeNetwork) _adrenalineModeNetwork.ClearReinforcementInput();
            if (network != _foragerModeNetwork) _foragerModeNetwork.ClearReinforcementInput();
            if (network != _deliverModeNetwork) _deliverModeNetwork.ClearReinforcementInput();
        }

        private void ForageState()
        {
            // Clear memory of other networks
            _adrenalineModeNetwork.ClearReinforcementInput();
            _deliverModeNetwork.ClearReinforcementInput();

            // Act on _foragerModeNetwork
            RunNetwork(_foragerModeNetwork);
        }

        private void DeliverState()
        {
            // Clear memory of other networks
            _foragerModeNetwork.ClearReinforcementInput();
            _adrenalineModeNetwork.ClearReinforcementInput();

            // Act on _foragerModeNetwork
            RunNetwork(_deliverModeNetwork);
        }

        private void AdrenalineState()
        {
            // TODO: WE HAVE AUTO ATTACK FOR THE MOMENT
            //// TODO: attack should also be an output state of the neuralnetwork
            //// Find something to attack
            //var creatureToAttack = MyCreature.FindCreatureToAttack(MyCreature.FoodSpecies);
            //if (creatureToAttack != null)
            //{
            //    MyCreature.Attack();
            //}

            // Clear memory of other networks
            _foragerModeNetwork.ClearReinforcementInput();
            _deliverModeNetwork.ClearReinforcementInput();

            // Act on _adrenalineModeNetwork
            RunNetwork(_adrenalineModeNetwork);
        }

        public override IBrain Replicate(IBrain mate)
        {
            // TODO: optimize
            // 'new' will also create a useless intial network

            // crossover
            var neuralMate = mate as NeuralBrain;
            Debug.Assert(neuralMate != null, "sodomy!");

            var newBrain = new NeuralBrain();
            newBrain._adrenalineModeNetwork = Globals.Radomizer.Next(1) == 0 ? _adrenalineModeNetwork.Replicate() : neuralMate._adrenalineModeNetwork.Replicate();
            newBrain._foragerModeNetwork = Globals.Radomizer.Next(1) == 0 ? _foragerModeNetwork.Replicate() : neuralMate._foragerModeNetwork.Replicate();
            newBrain._deliverModeNetwork = Globals.Radomizer.Next(1) == 0 ? _deliverModeNetwork.Replicate() : neuralMate._deliverModeNetwork.Replicate();
            newBrain._modeChoserNetwork = Globals.Radomizer.Next(1) == 0 ? _modeChoserNetwork.Replicate() : neuralMate._modeChoserNetwork.Replicate();

            newBrain.ConnectReinforcementNodes();

            return newBrain;
        }

        public override void Mutate()
        {
            Console.WriteLine("AdrenalineMode: ");
            _adrenalineModeNetwork.Mutate();
            Console.WriteLine("ForageMode: ");
            _foragerModeNetwork.Mutate();
            Console.WriteLine("DeliverMode: ");
            _deliverModeNetwork.Mutate();
            Console.WriteLine("ModeChoser: ");
            _modeChoserNetwork.Mutate();
        }

        public void Serialize(BinaryWriter writer)
        {
            _adrenalineModeNetwork.Serialize(writer);
            _foragerModeNetwork.Serialize(writer);
            _deliverModeNetwork.Serialize(writer);
            _modeChoserNetwork.Serialize(writer);
        }

        public void Deserialize(BinaryReader reader)
        {
            _adrenalineModeNetwork.Deserialize(reader);
            _foragerModeNetwork.Deserialize(reader);
            _deliverModeNetwork.Deserialize(reader);
            _modeChoserNetwork.Deserialize(reader);
        }
    }
}
