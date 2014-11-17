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
        private static int _standardError = 20;

        private IEye _forwardEye;
        private IEye _leftEye;
        private IEye _rightEye;

        private Dictionary<IEye, double> _eyeSeePrey = new Dictionary<IEye, double>();
        private Dictionary<IEye, double> _eyeSeeFood = new Dictionary<IEye, double>();
        private Dictionary<IEye, double> _eyeSeeWalls = new Dictionary<IEye, double>();
        private Dictionary<IEye, double> _eyeSeeDangers = new Dictionary<IEye, double>();
        private Dictionary<IEye, double> _eyeSeeFamily = new Dictionary<IEye, double>();
        //private Dictionary<IEye, double> _eyeSeeAll = new Dictionary<IEye, double>();
        private Dictionary<IEye, double> _eyeSeeMySpawnPoint = new Dictionary<IEye, double>();

        private IBumper _forwardBumper;
        private bool _initialized;

        private IEye _nose;
        private Dictionary<IEye, double> _smellMySpawnPoint = new Dictionary<IEye, double>();


        private const int _nrOfInputNodes = 25; // 3x eye x6, bumper, nose, 2x random, health, stamina, resources
        private const int _seePreyIndex = 0;
        private const int _seeFoodIndex = 3;
        private const int _seeWallsIndex = 6;
        private const int _seeMySpawnPointIndex = 9;
        private const int _seeDangersIndex = 12;
        private const int _seeFamilyIndex = 15;
        private const int _forwardBumperIndex = 18;
        private const int _smellMySpawnPointIndex = _forwardBumperIndex + 1;
        private const int _random1Index = _smellMySpawnPointIndex + 1;
        private const int _random2Index = _random1Index + 1;
        private const int _damageMonitorIndex = _random2Index + 1;
        private const int _fatigueMonitorIndex = _damageMonitorIndex + 1;
        private const int _resourceMonitorIndex = _fatigueMonitorIndex + 1;

        private const int _left = 0;
        private const int _forward = 1;
        private const int _right = 2;


        private const int _modeOutputNodes = 2; // Turn, Trust
        private NeuralNetwork _adrenalineModeNetwork;
        private NeuralNetwork _foragerModeNetwork;
        private NeuralNetwork _deliverModeNetwork;

        private NeuralNetwork _modeChoserNetwork;
        private const int _modeChoserOutputNodes = 3; // The modes

        public override void DoSomething(TimeSpan timeDelta)
        {
            Debug.Assert(MyCreature != null);
            Debug.Assert(_initialized);

            // Fill eye buffer
            SeePrey();
            SeeFood();
            SeeWalls();
            SeeDangers();
            SeeFamily();
            //SeeAll();
            SeeAndSmellSpawnPoint();


            var modeNetwork = ChoseCurrentMode();

            // Clear history of other networks. Otherwise you would get out of context state when you switch back to those networks
            ClearReinforcementOtherModeNetworks(modeNetwork);

            RunNetwork(modeNetwork);
        }

        private void SeePrey()
        {
            var entities = MyCreature.MyEnvironment.GetCreatures(MyCreature.FoodSpecies);
            var sortedOnDistance = FilterAndSortOnDistance(entities, MyCreature.CharacterSheet.VisionDistance);

            // Filter
            var filtered = new List<IEntity>();
            foreach (ICreature entity in sortedOnDistance)
            {
                // Not my family (not even for canibals)
                if (entity.SpawnPoint == MyCreature.SpawnPoint)
                    continue;

                filtered.Add(entity);
            }

            _eyeSeePrey.Clear();

            _eyeSeePrey.Add(_forwardEye, _forwardEye.WeightedDistanceToFirstVisible(filtered));
            _eyeSeePrey.Add(_leftEye, _leftEye.WeightedDistanceToFirstVisible(filtered));
            _eyeSeePrey.Add(_rightEye, _rightEye.WeightedDistanceToFirstVisible(filtered));
        }

        private void SeeFamily()
        {
            var entities = MyCreature.MyEnvironment.GetCreatures(MyCreature.CreatureType);
            var sortedOnDistance = FilterAndSortOnDistance(entities, MyCreature.CharacterSheet.VisionDistance);

            _eyeSeeFamily.Clear();

            _eyeSeeFamily.Add(_forwardEye, _forwardEye.WeightedDistanceToFirstVisible(sortedOnDistance));
            _eyeSeeFamily.Add(_leftEye, _leftEye.WeightedDistanceToFirstVisible(sortedOnDistance));
            _eyeSeeFamily.Add(_rightEye, _rightEye.WeightedDistanceToFirstVisible(sortedOnDistance));
        }

        private void SeeDangers()
        {
            var entities = MyCreature.MyEnvironment.GetCreatures().Where(e => e.FoodSpecies != null && e.FoodSpecies.Contains(MyCreature.CreatureType));
            var sortedOnDistance = FilterAndSortOnDistance(entities, MyCreature.CharacterSheet.VisionDistance);

            _eyeSeeDangers.Clear();

            _eyeSeeDangers.Add(_forwardEye, _forwardEye.WeightedDistanceToFirstVisible(sortedOnDistance));
            _eyeSeeDangers.Add(_leftEye, _leftEye.WeightedDistanceToFirstVisible(sortedOnDistance));
            _eyeSeeDangers.Add(_rightEye, _rightEye.WeightedDistanceToFirstVisible(sortedOnDistance));
        }

        private void SeeFood()
        {
            var entities = MyCreature.MyEnvironment.GetObstacles().Where(e => e.EntityType == EntityTypeEnum.Treasure && MyCreature.FoodSpecies.Contains(e.CreatureType));
            var sortedOnDistance = FilterAndSortOnDistance(entities, MyCreature.CharacterSheet.VisionDistance);

            _eyeSeeFood.Clear();

            _eyeSeeFood.Add(_forwardEye, _forwardEye.WeightedDistanceToFirstVisible(sortedOnDistance));
            _eyeSeeFood.Add(_leftEye, _leftEye.WeightedDistanceToFirstVisible(sortedOnDistance));
            _eyeSeeFood.Add(_rightEye, _rightEye.WeightedDistanceToFirstVisible(sortedOnDistance));
        }

        private void SeeWalls()
        {
            var entities = MyCreature.MyEnvironment.GetObstacles().Where(e => e.EntityType == EntityTypeEnum.Wall || e.EntityType == EntityTypeEnum.Box);
            var sortedOnDistance = FilterAndSortOnDistance(entities, MyCreature.CharacterSheet.VisionDistance);

            _eyeSeeWalls.Clear();

            _eyeSeeWalls.Add(_forwardEye, _forwardEye.WeightedDistanceToFirstVisible(sortedOnDistance, false));
            _eyeSeeWalls.Add(_leftEye, _leftEye.WeightedDistanceToFirstVisible(sortedOnDistance, false));
            _eyeSeeWalls.Add(_rightEye, _rightEye.WeightedDistanceToFirstVisible(sortedOnDistance, false));
        }

        private void SeeAndSmellSpawnPoint()
        {
            var entities = MyCreature.MyEnvironment.GetCreatures().Where(e => e.IsSpawnPoint);
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
            //network.InputNodes[_damageMonitorIndex].OutGoingEdges[1].Initialize(1);
            //network.InputNodes[_damageMonitorIndex].OutGoingEdges[2].Initialize(-.5);
            // > Fatigue
            // > Resources
            //network.InputNodes[_resourceMonitorIndex].OutGoingEdges[0].Initialize(-.5);
            network.InputNodes[_resourceMonitorIndex].OutGoingEdges[1].Initialize(10);
            //network.InputNodes[_resourceMonitorIndex].OutGoingEdges[2].Initialize(-1);


            network.LayerNodes[0].OutGoingEdges[0].Initialize(1); // ForagerMode
            network.LayerNodes[1].OutGoingEdges[1].Initialize(1); // DeliverMode
            network.LayerNodes[2].OutGoingEdges[2].Initialize(1); // AdrenalineMode


            // > ISeePrey
            network.ReinforcementInputNodes[_seePreyIndex + _left].OutGoingEdges[2].Initialize(.3);
            network.InputNodes[_seePreyIndex + _forward].OutGoingEdges[2].Initialize(1);
            network.ReinforcementInputNodes[_seePreyIndex + _right].OutGoingEdges[2].Initialize(.3);
            // > ISeeDanger
            //network.ReinforcementInputNodes[_seeDangersIndex + _left].OutGoingEdges[2].Initialize(.3);
            //network.ReinforcementInputNodes[_seeDangersIndex + _forward].OutGoingEdges[2].Initialize(1);
            //network.ReinforcementInputNodes[_seeDangersIndex + _right].OutGoingEdges[2].Initialize(.3);
            // > ISeeTreasure
            network.InputNodes[_seeFoodIndex + _forward].OutGoingEdges[0].Initialize(1);
            // > ISeeMySpawnPoint
            network.InputNodes[_seeMySpawnPointIndex + _forward].OutGoingEdges[1].Initialize(1);



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
            network.ReinforcementInputNodes[_random1Index].OutGoingEdges[0].Initialize(.1);


            return network;
        }

        private static NeuralNetwork PredefineAdrenalineMode(int inputNodes, int outputNodes)
        {
            var network = new NeuralNetwork(inputNodes, inputNodes * 2, outputNodes + inputNodes, inputNodes);

            // Eyes
            // > ISeePrey => attack prey
            network.InputNodes[_seePreyIndex + _left].OutGoingEdges[0].Initialize(1.5);
            network.InputNodes[_seePreyIndex + _forward].OutGoingEdges[1].Initialize(1.5);
            network.InputNodes[_seePreyIndex + _right].OutGoingEdges[2].Initialize(1.5);
            // > ISeeDanger => run from danger
            network.InputNodes[_seeDangersIndex + _left].OutGoingEdges[0].Initialize(-0.5);
            //network.InputNodes[_seeDangersIndex + _forward].OutGoingEdges[1].Initialize(-1.0);
            network.InputNodes[_seeDangersIndex + _right].OutGoingEdges[2].Initialize(-0.5);
            // > ISeeFamily => seek help
            //network.InputNodes[_seeFamilyIndex + _left].OutGoingEdges[0].Initialize(0.5);
            //network.InputNodes[_seeFamilyIndex + _forward].OutGoingEdges[1].Initialize(1);
            //network.InputNodes[_seeFamilyIndex + _right].OutGoingEdges[2].Initialize(0.5);

            network.LayerNodes[0].OutGoingEdges[0].Initialize(-1.5);
            network.LayerNodes[1].OutGoingEdges[1].Initialize(1.5);
            network.LayerNodes[2].OutGoingEdges[0].Initialize(1.5);

            // bumper


            // Reinforcement
            BuildStandardReinforcementSetup(network);

            network.ReinforcementInputNodes[_seePreyIndex + _left].OutGoingEdges[0].Initialize(0.5);
            network.ReinforcementInputNodes[_seePreyIndex + _forward].OutGoingEdges[1].Initialize(0.5);
            network.ReinforcementInputNodes[_seePreyIndex + _right].OutGoingEdges[2].Initialize(0.5);

            return network;
        }

        private static NeuralNetwork PredefineForagerMode(int inputNodes, int outputNodes)
        {
            var network = new NeuralNetwork(inputNodes, inputNodes*2, outputNodes + inputNodes, inputNodes);

            // eyes
            // > ISeeTreasure
            network.InputNodes[_seeFoodIndex + _left].OutGoingEdges[0].Initialize(1);
            network.InputNodes[_seeFoodIndex + _forward].OutGoingEdges[1].Initialize(1);
            network.InputNodes[_seeFoodIndex + _right].OutGoingEdges[2].Initialize(1);

            // > ISeeDanger => run from danger
            network.InputNodes[_seeDangersIndex + _left].OutGoingEdges[0].Initialize(-0.5);
            //network.InputNodes[_seeDangersIndex + _forward].OutGoingEdges[1].Initialize(-0.5);
            network.InputNodes[_seeDangersIndex + _right].OutGoingEdges[2].Initialize(-0.5);


            network.LayerNodes[0].OutGoingEdges[0].Initialize(-1);
            network.LayerNodes[1].OutGoingEdges[1].Initialize(1);
            network.LayerNodes[2].OutGoingEdges[0].Initialize(1);


            // Reinforcement
            BuildStandardReinforcementSetup(network);


            // bumper
            network.ReinforcementInputNodes[_forwardBumperIndex].OutGoingEdges[0].Initialize(0.5);
            network.ReinforcementInputNodes[_forwardBumperIndex].OutGoingEdges[1].Initialize(-2);
            network.ReinforcementInputNodes[_forwardBumperIndex].OutGoingEdges[2].Initialize(-0.5);

            // random nodes

            // reinforced random: input nodes
            network.ReinforcementInputNodes[_random1Index].OutGoingEdges[0].Initialize(.1);
            network.ReinforcementInputNodes[_random1Index].OutGoingEdges[1].Initialize(.3);
            network.ReinforcementInputNodes[_random1Index].OutGoingEdges[2].Initialize(-.1);

            network.ReinforcementInputNodes[_random2Index].OutGoingEdges[0].Initialize(-.1);
            network.ReinforcementInputNodes[_random2Index].OutGoingEdges[1].Initialize(.3);
            network.ReinforcementInputNodes[_random2Index].OutGoingEdges[2].Initialize(.1);

            return network;
        }

        private static NeuralNetwork PredefineDeliverMode(int inputNodes, int outputNodes)
        {
            var network = new NeuralNetwork(inputNodes, inputNodes*2, outputNodes + inputNodes, inputNodes);

            // eyes
            // > ISeeMySpawnPoint
            network.InputNodes[_seeMySpawnPointIndex + _left].OutGoingEdges[0].Initialize(1);
            network.InputNodes[_seeMySpawnPointIndex + _forward].OutGoingEdges[1].Initialize(1);
            network.InputNodes[_seeMySpawnPointIndex + _right].OutGoingEdges[2].Initialize(1);
            // > ISeeFamily
            network.InputNodes[_seeFamilyIndex + _left].OutGoingEdges[0].Initialize(0.3);
            network.InputNodes[_seeFamilyIndex + _forward].OutGoingEdges[1].Initialize(0.3);
            network.InputNodes[_seeFamilyIndex + _right].OutGoingEdges[2].Initialize(0.3);

            network.LayerNodes[0].OutGoingEdges[0].Initialize(-2);
            network.LayerNodes[1].OutGoingEdges[1].Initialize(2);
            network.LayerNodes[2].OutGoingEdges[0].Initialize(2);


            // Reinforcement
            BuildStandardReinforcementSetup(network);


            // bumper
            network.ReinforcementInputNodes[_forwardBumperIndex].OutGoingEdges[0].Initialize(0.5);
            network.ReinforcementInputNodes[_forwardBumperIndex].OutGoingEdges[1].Initialize(-2);
            network.ReinforcementInputNodes[_forwardBumperIndex].OutGoingEdges[2].Initialize(-0.5);


            // Smells like home
            network.InputNodes[_smellMySpawnPointIndex].OutGoingEdges[1].Initialize(5);

            // random nodes

            // reinforced random: input nodes
            network.ReinforcementInputNodes[_random1Index].OutGoingEdges[0].Initialize(.1);
            network.ReinforcementInputNodes[_random2Index].OutGoingEdges[1].Initialize(.1); 
            
            return network;
        }

        internal void PredefineBehaviour()
        {
            // Masterbrain
            _modeChoserNetwork = PredefineModeChoser(_nrOfInputNodes, _modeChoserOutputNodes);

            // AdrenalineMode
            _adrenalineModeNetwork = PredefineAdrenalineMode(_nrOfInputNodes, _modeOutputNodes);

            // ForagerMode
            _foragerModeNetwork = PredefineForagerMode(_nrOfInputNodes, _modeOutputNodes);

            // DeliverMode
            _deliverModeNetwork = PredefineDeliverMode(_nrOfInputNodes, _modeOutputNodes);

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

            for (int i = 0; i < _nrOfInputNodes; i++)
            {
                _modeChoserNetwork.ReinforcementInputNodes[i].NodeToReinforce = (_modeChoserNetwork.OutputNodes[_modeChoserOutputNodes + i]);
            }

            for (int i = 0; i < _nrOfInputNodes; i++)
            {
                _adrenalineModeNetwork.ReinforcementInputNodes[i].NodeToReinforce = (_adrenalineModeNetwork.OutputNodes[_modeOutputNodes+i]);
            }
            for (int i = 0; i < _nrOfInputNodes; i++)
            {
                _foragerModeNetwork.ReinforcementInputNodes[i].NodeToReinforce = (_foragerModeNetwork.OutputNodes[_modeOutputNodes+i]);
            }
            for (int i = 0; i < _nrOfInputNodes; i++)
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
            // seePrey
            {
                network.InputNodes[_seePreyIndex + _left].SetValue(_eyeSeePrey[_leftEye]);
                network.InputNodes[_seePreyIndex + _forward].SetValue(_eyeSeePrey[_forwardEye]);
                network.InputNodes[_seePreyIndex + _right].SetValue(_eyeSeePrey[_rightEye]);
            }
            // seeFood
            {
                network.InputNodes[_seeFoodIndex + _left].SetValue(_eyeSeeFood[_leftEye]);
                network.InputNodes[_seeFoodIndex + _forward].SetValue(_eyeSeeFood[_forwardEye]);
                network.InputNodes[_seeFoodIndex + _right].SetValue(_eyeSeeFood[_rightEye]);
            }
            // seeWalls
            {
                network.InputNodes[_seeWallsIndex + _left].SetValue(_eyeSeeWalls[_leftEye]);
                network.InputNodes[_seeWallsIndex + _forward].SetValue(_eyeSeeWalls[_forwardEye]);
                network.InputNodes[_seeWallsIndex + _right].SetValue(_eyeSeeWalls[_rightEye]);
            }
            // seeMySpawnPoint
            {
                network.InputNodes[_seeMySpawnPointIndex + _left].SetValue(_eyeSeeMySpawnPoint[_leftEye]);
                network.InputNodes[_seeMySpawnPointIndex + _forward].SetValue(_eyeSeeMySpawnPoint[_forwardEye]);
                network.InputNodes[_seeMySpawnPointIndex + _right].SetValue(_eyeSeeMySpawnPoint[_rightEye]);
            }
            // seeDangers
            {
                network.InputNodes[_seeDangersIndex + _left].SetValue(_eyeSeeDangers[_leftEye]);
                network.InputNodes[_seeDangersIndex + _forward].SetValue(_eyeSeeDangers[_forwardEye]);
                network.InputNodes[_seeDangersIndex + _right].SetValue(_eyeSeeDangers[_rightEye]);
            }
            // seeFamily
            {
                network.InputNodes[_seeFamilyIndex + _left].SetValue(_eyeSeeFamily[_leftEye]);
                network.InputNodes[_seeFamilyIndex + _forward].SetValue(_eyeSeeFamily[_forwardEye]);
                network.InputNodes[_seeFamilyIndex + _right].SetValue(_eyeSeeFamily[_rightEye]);
            }
            network.InputNodes[_forwardBumperIndex].SetValue(_forwardBumper.Hit ? 100 : 0);
            network.InputNodes[_smellMySpawnPointIndex].SetValue(_smellMySpawnPoint[_nose]);
            network.InputNodes[_random1Index].SetValue(Globals.Radomizer.Next(50));
            network.InputNodes[_random2Index].SetValue(Globals.Radomizer.Next(50));
            network.InputNodes[_damageMonitorIndex].SetValue(this.MyCreature.CharacterSheet.Damage.PercentFilled);
            network.InputNodes[_fatigueMonitorIndex].SetValue(this.MyCreature.CharacterSheet.Fatigue.PercentFilled);
            network.InputNodes[_resourceMonitorIndex].SetValue(this.MyCreature.CharacterSheet.Resource.PercentFilled);
        }

        private void RunNetwork(NeuralNetwork network)
        {
            FillInputNodes(network);

            // Process
            network.FuzzyPropagate(_standardError);

            // Feed output to creature
            MyCreature.Turn(network.OutputNodes[0].GetValue() / 100);
            MyCreature.Thrust(network.OutputNodes[1].GetValue() / 100);

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
            _modeChoserNetwork.FuzzyPropagate(_standardError);

            var forageWeight = _modeChoserNetwork.OutputNodes[0].GetValue();
            var deliverWeight = _modeChoserNetwork.OutputNodes[1].GetValue();
            var adrenalineWeight = _modeChoserNetwork.OutputNodes[2].GetValue();

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
