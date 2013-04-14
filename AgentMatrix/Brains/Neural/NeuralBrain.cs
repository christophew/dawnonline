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
        protected IEye _forwardEye;
        protected IEye _leftEye;
        protected IEye _rightEye;

        protected Dictionary<IEye, double> _eyeSeeEnemy = new Dictionary<IEye, double>();
        protected Dictionary<IEye, double> _eyeSeeTreasure = new Dictionary<IEye, double>();
        protected Dictionary<IEye, double> _eyeSeeWalls = new Dictionary<IEye, double>();
        protected Dictionary<IEye, double> _eyeSeeMySpawnPoint = new Dictionary<IEye, double>();

        protected IBumper _forwardBumper;
        protected bool _initialized;



        private NeuralNetwork _adrenalineModeNetwork;
        private const int _adrenalineInputNodes = 17; // 3x eye x4, bumper, health, stamina, 2x random
        private const int _adrenalineOutputNodes = 4; 

        private NeuralNetwork _foragerModeNetwork;
        private const int _foragerInputNodes = 17; // 3x eye x4, bumper, health, stamina, 2x random
        private const int _foragerOutputNodes = 4;

        private NeuralNetwork _deliverModeNetwork;
        private const int _deliverInputNodes = 17; // 3x eye x4, bumper, health, stamina, 2x random
        private const int _deliverOutputNodes = 4;



        public override void DoSomething(TimeSpan timeDelta)
        {
            Debug.Assert(MyCreature != null);
            Debug.Assert(_initialized);

            // Fill eye buffer
            SeeEnemies();
            SeeTreasures();
            SeeWalls();
            SeeSpawnPoint();

            // Emotional states
            // * neutral
            // * adrenaline
            // * fear
            // * tired


            // I have resources => deliver
            if (MyCreature.CharacterSheet.Resource.PercentFilled > 5)
            {
                DeliverState(timeDelta);
                return;
            }

            //if (ISeeAnEnemy())
            //{
            //    // CHAAAAARGE!
            //    AdrenalineState(timeDelta);
            //    return;
            //}

            ForageState(timeDelta);
        }

        private bool ISeeAnEnemy()
        {
            //var check = _eyeSee[_forwardEye] > 0 || _eyeSee[_leftEye] > 0 || _eyeSee[_rightEye] > 0;
            // get a good look!
            var check = _eyeSeeEnemy[_forwardEye] > 5 || _eyeSeeEnemy[_leftEye] > 5 || _eyeSeeEnemy[_rightEye] > 5;

            return check;
        }

        private void SeeEnemies()
        {
            var entities = MyCreature.MyEnvironment.GetCreatures(MyCreature.FoodSpecies);
            var sortedOnDistance = FilterAndSortOnDistance(entities);

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

            _eyeSeeEnemy.Add(_forwardEye, _forwardEye.DistanceToFirstVisible(filtered));
            _eyeSeeEnemy.Add(_leftEye, _leftEye.DistanceToFirstVisible(filtered));
            _eyeSeeEnemy.Add(_rightEye, _rightEye.DistanceToFirstVisible(filtered));
        }

        private void SeeTreasures()
        {
            var entities = MyCreature.MyEnvironment.GetObstacles().Where(e => e.Specy == EntityType.Treasure);
            var sortedOnDistance = FilterAndSortOnDistance(entities);

            _eyeSeeTreasure.Clear();

            _eyeSeeTreasure.Add(_forwardEye, _forwardEye.DistanceToFirstVisible(sortedOnDistance));
            _eyeSeeTreasure.Add(_leftEye, _leftEye.DistanceToFirstVisible(sortedOnDistance));
            _eyeSeeTreasure.Add(_rightEye, _rightEye.DistanceToFirstVisible(sortedOnDistance));
        }

        private void SeeWalls()
        {
            var entities = MyCreature.MyEnvironment.GetObstacles().Where(e => e.Specy == EntityType.Wall || e.Specy == EntityType.Box);
            var sortedOnDistance = FilterAndSortOnDistance(entities);

            _eyeSeeWalls.Clear();

            _eyeSeeWalls.Add(_forwardEye, _forwardEye.DistanceToFirstVisible(sortedOnDistance, false));
            _eyeSeeWalls.Add(_leftEye, _leftEye.DistanceToFirstVisible(sortedOnDistance, false));
            _eyeSeeWalls.Add(_rightEye, _rightEye.DistanceToFirstVisible(sortedOnDistance, false));
        }

        private void SeeSpawnPoint()
        {
            var entities = MyCreature.MyEnvironment.GetCreatures().Where(e => e.Specy == EntityType.SpawnPoint);
            var sortedOnDistance = FilterAndSortOnDistance(entities);

            // Filter
            var filtered = new List<IEntity>();
            foreach (ICreature entity in sortedOnDistance)
            {
                // Not my enemy
                if (entity.SpawnPoint != MyCreature.SpawnPoint)
                    continue;

                filtered.Add(entity);
            }

            _eyeSeeMySpawnPoint.Clear();

            _eyeSeeMySpawnPoint.Add(_forwardEye, _forwardEye.DistanceToFirstVisible(filtered, false));
            _eyeSeeMySpawnPoint.Add(_leftEye, _leftEye.DistanceToFirstVisible(filtered, false));
            _eyeSeeMySpawnPoint.Add(_rightEye, _rightEye.DistanceToFirstVisible(filtered, false));
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


            _initialized = true;
        }

        internal void PredefineBehaviour()
        {
            // AdrenalineMode
            {
                // 1 reinforcement node foreach regular input node
                _adrenalineModeNetwork = new NeuralNetwork(_adrenalineInputNodes, _adrenalineInputNodes * 2, _adrenalineOutputNodes + _adrenalineInputNodes, _adrenalineInputNodes);

                // Eyes
                // > ISeeEnemy
                _adrenalineModeNetwork.InputNodes[0].OutGoingEdges[0].Initialize(1.5);
                _adrenalineModeNetwork.InputNodes[1].OutGoingEdges[1].Initialize(1.5);
                _adrenalineModeNetwork.InputNodes[2].OutGoingEdges[2].Initialize(1.5);

                _adrenalineModeNetwork.LayerNodes[0].OutGoingEdges[0].Initialize(-1.5);
                _adrenalineModeNetwork.LayerNodes[1].OutGoingEdges[1].Initialize(1.5);
                _adrenalineModeNetwork.LayerNodes[2].OutGoingEdges[0].Initialize(1.5);

                // bumper


                // Reinforcement
                BuildStandardReinforcementSetup(_adrenalineModeNetwork);

                _adrenalineModeNetwork.ReinforcementInputNodes[0].OutGoingEdges[0].Initialize(0.5);
                _adrenalineModeNetwork.ReinforcementInputNodes[1].OutGoingEdges[1].Initialize(0.5);
                _adrenalineModeNetwork.ReinforcementInputNodes[2].OutGoingEdges[2].Initialize(0.5);

            }

            // ForagerMode
            {
                 // 1 reinforcement node foreach regular input node
               _foragerModeNetwork = new NeuralNetwork(_foragerInputNodes, _foragerInputNodes * 2, _foragerOutputNodes + _foragerInputNodes, _foragerInputNodes);

                // eyes
               // > ISeeTreasure
               _foragerModeNetwork.InputNodes[3].OutGoingEdges[0].Initialize(1);
                _foragerModeNetwork.InputNodes[4].OutGoingEdges[1].Initialize(1);
                _foragerModeNetwork.InputNodes[5].OutGoingEdges[2].Initialize(1);

                _foragerModeNetwork.LayerNodes[0].OutGoingEdges[0].Initialize(-1);
                _foragerModeNetwork.LayerNodes[1].OutGoingEdges[1].Initialize(1);
                _foragerModeNetwork.LayerNodes[2].OutGoingEdges[0].Initialize(1);


                // Reinforcement
                BuildStandardReinforcementSetup(_foragerModeNetwork);


                // bumper
                _foragerModeNetwork.ReinforcementInputNodes[12].OutGoingEdges[0].Initialize(0.5);
                _foragerModeNetwork.ReinforcementInputNodes[12].OutGoingEdges[1].Initialize(-2); 
                _foragerModeNetwork.ReinforcementInputNodes[12].OutGoingEdges[2].Initialize(-0.5); 

                // random nodes

                // reinforced random: input nodes
                _foragerModeNetwork.ReinforcementInputNodes[13].OutGoingEdges[0].Initialize(.1);
                _foragerModeNetwork.ReinforcementInputNodes[13].OutGoingEdges[1].Initialize(.3);
                _foragerModeNetwork.ReinforcementInputNodes[13].OutGoingEdges[2].Initialize(-.1);

                _foragerModeNetwork.ReinforcementInputNodes[14].OutGoingEdges[0].Initialize(-.1);
                _foragerModeNetwork.ReinforcementInputNodes[14].OutGoingEdges[1].Initialize(.3);
                _foragerModeNetwork.ReinforcementInputNodes[14].OutGoingEdges[2].Initialize(.1);
            }

            // DeliverMode
            {
                 // 1 reinforcement node foreach regular input node
                _deliverModeNetwork = new NeuralNetwork(_deliverInputNodes, _deliverInputNodes * 2, _deliverOutputNodes + _deliverInputNodes, _deliverInputNodes);

                // eyes
                // > ISeeMySpawnPoint
                _deliverModeNetwork.InputNodes[9].OutGoingEdges[0].Initialize(1);
                _deliverModeNetwork.InputNodes[10].OutGoingEdges[1].Initialize(1);
                _deliverModeNetwork.InputNodes[11].OutGoingEdges[2].Initialize(1);

                _deliverModeNetwork.LayerNodes[0].OutGoingEdges[0].Initialize(-2);
                _deliverModeNetwork.LayerNodes[1].OutGoingEdges[1].Initialize(2);
                _deliverModeNetwork.LayerNodes[2].OutGoingEdges[0].Initialize(2);


                // Reinforcement
                BuildStandardReinforcementSetup(_deliverModeNetwork);


                // bumper
                _deliverModeNetwork.ReinforcementInputNodes[12].OutGoingEdges[0].Initialize(0.5);
                _deliverModeNetwork.ReinforcementInputNodes[12].OutGoingEdges[1].Initialize(-2);
                _deliverModeNetwork.ReinforcementInputNodes[12].OutGoingEdges[2].Initialize(-0.5); 

                // random nodes

                // reinforced random: input nodes
                _deliverModeNetwork.ReinforcementInputNodes[13].OutGoingEdges[0].Initialize(.1);
                _deliverModeNetwork.ReinforcementInputNodes[13].OutGoingEdges[1].Initialize(.2);
                //_deliverModeNetwork.ReinforcementInputNodes[13].OutGoingEdges[2].Initialize(-.1);

                //_deliverModeNetwork.ReinforcementInputNodes[14].OutGoingEdges[0].Initialize(-.1);
                //_deliverModeNetwork.ReinforcementInputNodes[14].OutGoingEdges[1].Initialize(.3);
                //_deliverModeNetwork.ReinforcementInputNodes[14].OutGoingEdges[2].Initialize(.1);
            }

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
            for (int i = 0; i < _adrenalineInputNodes; i++)
            {
                _adrenalineModeNetwork.ReinforcementInputNodes[i].NodeToReinforce = (_adrenalineModeNetwork.OutputNodes[_adrenalineOutputNodes+i]);
            }
            for (int i = 0; i < _foragerInputNodes; i++)
            {
                _foragerModeNetwork.ReinforcementInputNodes[i].NodeToReinforce = (_foragerModeNetwork.OutputNodes[_foragerOutputNodes+i]);
            }
            for (int i = 0; i < _deliverInputNodes; i++)
            {
                _deliverModeNetwork.ReinforcementInputNodes[i].NodeToReinforce = (_deliverModeNetwork.OutputNodes[_deliverOutputNodes + i]);
            }
        }

        public override void ClearState()
        {
            _forwardBumper.Clear();

            _adrenalineModeNetwork.ClearInput();
            _foragerModeNetwork.ClearInput();
            _deliverModeNetwork.ClearInput();
        }

        private static double GetEyeCheck(IEye eye, double value)
        {
            return value < 0 ? 0 : 100.0*(eye.VisionDistance - value)/eye.VisionDistance;
        }

        private void RunNetwork(NeuralNetwork network, TimeSpan timeDelta)
        {
            int i = 0;
            if (_eyeSeeEnemy.Count > 0)
            {
                network.InputNodes[i++].CurrentValue = GetEyeCheck(_leftEye, _eyeSeeEnemy[_leftEye]);
                network.InputNodes[i++].CurrentValue = GetEyeCheck(_forwardEye, _eyeSeeEnemy[_forwardEye]);
                network.InputNodes[i++].CurrentValue = GetEyeCheck(_rightEye, _eyeSeeEnemy[_rightEye]);
            }
            else i += 3;
            if (_eyeSeeTreasure.Count > 0)
            {
                network.InputNodes[i++].CurrentValue = GetEyeCheck(_leftEye, _eyeSeeTreasure[_leftEye]);
                network.InputNodes[i++].CurrentValue = GetEyeCheck(_forwardEye, _eyeSeeTreasure[_forwardEye]);
                network.InputNodes[i++].CurrentValue = GetEyeCheck(_rightEye, _eyeSeeTreasure[_rightEye]);
            }
            else i += 3;
            if (_eyeSeeWalls.Count > 0)
            {
                network.InputNodes[i++].CurrentValue = GetEyeCheck(_leftEye, _eyeSeeWalls[_leftEye]);
                network.InputNodes[i++].CurrentValue = GetEyeCheck(_forwardEye, _eyeSeeWalls[_forwardEye]);
                network.InputNodes[i++].CurrentValue = GetEyeCheck(_rightEye, _eyeSeeWalls[_rightEye]);
            }
            else i += 3;
            if (_eyeSeeMySpawnPoint.Count > 0)
            {
                network.InputNodes[i++].CurrentValue = GetEyeCheck(_leftEye, _eyeSeeMySpawnPoint[_leftEye]);
                network.InputNodes[i++].CurrentValue = GetEyeCheck(_forwardEye, _eyeSeeMySpawnPoint[_forwardEye]);
                network.InputNodes[i++].CurrentValue = GetEyeCheck(_rightEye, _eyeSeeMySpawnPoint[_rightEye]);
            }
            else i += 3;
            Debug.Assert(i == 12);
            network.InputNodes[i++].CurrentValue = _forwardBumper.Hit ? 100 : 0;
            network.InputNodes[i++].CurrentValue = Globals.Radomizer.Next(50);
            network.InputNodes[i++].CurrentValue = Globals.Radomizer.Next(50);
            network.InputNodes[i++].CurrentValue = this.MyCreature.CharacterSheet.Damage.PercentFilled;
            network.InputNodes[i++].CurrentValue = this.MyCreature.CharacterSheet.Fatigue.PercentFilled;
            Debug.Assert(i == 17);

            //network.InputNodes[i++].CurrentValue = _leftEar.HearFamily(Sound.SoundTypeEnum.A);
            //network.InputNodes[i++].CurrentValue = _leftEar.HearStrangers(Sound.SoundTypeEnum.A);
            //network.InputNodes[i++].CurrentValue = _leftEar.HearFamily(Sound.SoundTypeEnum.B);
            //network.InputNodes[i++].CurrentValue = _leftEar.HearStrangers(Sound.SoundTypeEnum.B);
            //Debug.Assert(i == 18);

            // Process
            network.Propagate(timeDelta);

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

        private void ForageState(TimeSpan timeDelta)
        {
            // Clear memory of other networks
            _adrenalineModeNetwork.ClearReinforcementInput();
            _deliverModeNetwork.ClearReinforcementInput();

            // Act on _foragerModeNetwork
            RunNetwork(_foragerModeNetwork, timeDelta);
        }

        private void DeliverState(TimeSpan timeDelta)
        {
            // Clear memory of other networks
            _foragerModeNetwork.ClearReinforcementInput();
            _adrenalineModeNetwork.ClearReinforcementInput();

            // Act on _foragerModeNetwork
            RunNetwork(_deliverModeNetwork, timeDelta);
        }

        private void AdrenalineState(TimeSpan timeDelta)
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
            RunNetwork(_adrenalineModeNetwork, timeDelta);
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
        }

        public void Serialize(BinaryWriter writer)
        {
            _adrenalineModeNetwork.Serialize(writer);
            _foragerModeNetwork.Serialize(writer);
            _deliverModeNetwork.Serialize(writer);
        }

        public void Deserialize(BinaryReader reader)
        {
            _adrenalineModeNetwork.Deserialize(reader);
            _foragerModeNetwork.Deserialize(reader);
            _deliverModeNetwork.Deserialize(reader);
        }
    }
}
