using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Brains;
using DawnOnline.Simulation.Entities;

namespace DawnOnline.Simulation.Brains.Neural
{
    internal class NeuralBrain : ForagerBrain
    {
        private NeuralNetwork _adrenalineModeNetwork;
        private int _adrenalineInputNodes = 6; // 3x eye, bumper, 2x random
        private int _adrenalineOutputNodes = 2; // 3x eye, bumper, 2x random
        private NeuralNetwork _foragerModeNetwork;
        private int _foragerInputNodes = 6;
        private int _foragerOutputNodes = 2;

        internal NeuralBrain()
        {
            // AdrenalineMode
            {
                // 1 reinforcement node foreach regular input node
                _adrenalineModeNetwork = new NeuralNetwork(_adrenalineInputNodes, _adrenalineInputNodes * 2, _adrenalineOutputNodes + _adrenalineInputNodes, _adrenalineInputNodes);

                // Eyes
                _adrenalineModeNetwork.InputNodes[0].OutGoingEdges[0].Multiplier = 1.5;
                _adrenalineModeNetwork.InputNodes[1].OutGoingEdges[1].Multiplier = 1.5;
                _adrenalineModeNetwork.InputNodes[2].OutGoingEdges[2].Multiplier = 1.5;

                _adrenalineModeNetwork.LayerNodes[0].OutGoingEdges[0].Multiplier = -1.5;
                _adrenalineModeNetwork.LayerNodes[1].OutGoingEdges[1].Multiplier = 1.5;
                _adrenalineModeNetwork.LayerNodes[2].OutGoingEdges[0].Multiplier = 1.5;

                // bumper


                // Reinforcement
                BuildStandardReinforcementSetup(_adrenalineModeNetwork);

                _adrenalineModeNetwork.ReinforcementInputNodes[0].OutGoingEdges[0].Multiplier = 0.5;
                _adrenalineModeNetwork.ReinforcementInputNodes[1].OutGoingEdges[1].Multiplier = 0.5;
                _adrenalineModeNetwork.ReinforcementInputNodes[2].OutGoingEdges[2].Multiplier = 0.5;

            }

            // NeutralMode
            {
                 // 1 reinforcement node foreach regular input node
               _foragerModeNetwork = new NeuralNetwork(_foragerInputNodes, _foragerInputNodes * 2, _foragerOutputNodes + _foragerInputNodes, _foragerInputNodes);

                // eyes
                _foragerModeNetwork.InputNodes[0].OutGoingEdges[0].Multiplier = 1;
                _foragerModeNetwork.InputNodes[1].OutGoingEdges[1].Multiplier = 1;
                _foragerModeNetwork.InputNodes[2].OutGoingEdges[2].Multiplier = 1;

                _foragerModeNetwork.LayerNodes[0].OutGoingEdges[0].Multiplier = -1;
                _foragerModeNetwork.LayerNodes[1].OutGoingEdges[1].Multiplier = 1;
                _foragerModeNetwork.LayerNodes[2].OutGoingEdges[0].Multiplier = 1;

                // Reinforcement
                BuildStandardReinforcementSetup(_foragerModeNetwork);


                // bumper
                _foragerModeNetwork.ReinforcementInputNodes[3].OutGoingEdges[0].Multiplier = 0.5;
                _foragerModeNetwork.ReinforcementInputNodes[3].OutGoingEdges[1].Multiplier = -2; 
                _foragerModeNetwork.ReinforcementInputNodes[3].OutGoingEdges[2].Multiplier = -0.5; 

                // random nodes

                // reinforced random: input nodes [4, 5]
                _foragerModeNetwork.ReinforcementInputNodes[4].OutGoingEdges[0].Multiplier = .1;
                _foragerModeNetwork.ReinforcementInputNodes[4].OutGoingEdges[1].Multiplier = .3;
                _foragerModeNetwork.ReinforcementInputNodes[4].OutGoingEdges[2].Multiplier = -.1;

                _foragerModeNetwork.ReinforcementInputNodes[5].OutGoingEdges[0].Multiplier = -.1;
                _foragerModeNetwork.ReinforcementInputNodes[5].OutGoingEdges[1].Multiplier = .3;
                _foragerModeNetwork.ReinforcementInputNodes[5].OutGoingEdges[2].Multiplier = .1;
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
                network.InputNodes[i].OutGoingEdges[network.InputNodes.Length + i].Multiplier = originalDecay;
            }
            // standard reinforcement setup: small decay
            const double decay = 0.9;
            var nrOfOutputNodes = network.OutputNodes.Length - network.ReinforcementInputNodes.Length;
            for (int i = 0; i < network.ReinforcementInputNodes.Length; i++)
            {
                network.ReinforcementInputNodes[i].OutGoingEdges[network.InputNodes.Length + i].Multiplier = decay;
                network.LayerNodes[network.InputNodes.Length + i].OutGoingEdges[nrOfOutputNodes + i].Multiplier = decay;
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
        }

        internal override void ClearState()
        {
            base.ClearState();

            _adrenalineModeNetwork.Reset();
            _foragerModeNetwork.Reset();
        }

        protected override void NeutralState(TimeSpan timeDelta)
        {
            // Forage
            var leftEyeCheck = _leftEye.SeesAnObstacle(EntityType.Treasure);
            var forwardEyeCheck = _forwardEye.SeesAnObstacle(EntityType.Treasure);
            var rightEyeCheck = _rightEye.SeesAnObstacle(EntityType.Treasure);

            _foragerModeNetwork.InputNodes[0].CurrentValue = leftEyeCheck ? 50 : 0;
            _foragerModeNetwork.InputNodes[1].CurrentValue = forwardEyeCheck ? 50 : 0;
            _foragerModeNetwork.InputNodes[2].CurrentValue = rightEyeCheck ? 50 : 0;
            _foragerModeNetwork.InputNodes[3].CurrentValue = _forwardBumper.Hit ? 100 : 0;
            _foragerModeNetwork.InputNodes[4].CurrentValue = Globals.Radomizer.Next(50);
            _foragerModeNetwork.InputNodes[5].CurrentValue = Globals.Radomizer.Next(50);

            // Process
            _foragerModeNetwork.Propagate(timeDelta);

            // Feed output to creature
            MyCreature.Turn(_foragerModeNetwork.OutputNodes[0].CurrentValue / 100);
            MyCreature.Thrust(_foragerModeNetwork.OutputNodes[1].CurrentValue / 100);




            //// TODO: neural equivalent
            //if (Math.Abs(_foragerModeNetwork.OutputNodes[0].CurrentValue) < 5 &&
            //    Math.Abs(_foragerModeNetwork.OutputNodes[1].CurrentValue) < 5)
            //{
            //    // TODO: use random input node
            //    DoRandomAction(1000);
            //}
        }

        protected override void AdrenalineState(TimeSpan timeDelta)
        {
            // Find something to attack
            var creaturesToAttack = MyCreature.FindCreatureToAttack(MyCreature.FoodSpecies);
            if (creaturesToAttack != null)
            {
                MyCreature.Attack(creaturesToAttack);
            }

            // Init network with input values
            var leftEyeCheck = _eyeSee[_leftEye] < 0 ? 0 : 100.0 * (_leftEye.VisionDistance - _eyeSee[_leftEye]) / _leftEye.VisionDistance;
            var forwardEyeCheck = _eyeSee[_forwardEye] < 0 ? 0 : 100.0 * (_forwardEye.VisionDistance - _eyeSee[_forwardEye]) / _forwardEye.VisionDistance;
            var rightEyeCheck = _eyeSee[_rightEye] < 0 ? 0 : 100.0 * (_rightEye.VisionDistance - _eyeSee[_rightEye]) / _rightEye.VisionDistance;

            _adrenalineModeNetwork.InputNodes[0].CurrentValue = leftEyeCheck;
            _adrenalineModeNetwork.InputNodes[1].CurrentValue = forwardEyeCheck;
            _adrenalineModeNetwork.InputNodes[2].CurrentValue = rightEyeCheck;
            _adrenalineModeNetwork.InputNodes[3].CurrentValue = _forwardBumper.Hit ? 100 : 0;
            _adrenalineModeNetwork.InputNodes[4].CurrentValue = Globals.Radomizer.Next(50);
            _adrenalineModeNetwork.InputNodes[5].CurrentValue = Globals.Radomizer.Next(50);

            // Process
            _adrenalineModeNetwork.Propagate(timeDelta);

            // Feed output to creature
            MyCreature.Turn(_adrenalineModeNetwork.OutputNodes[0].CurrentValue / 100);
            MyCreature.Thrust(_adrenalineModeNetwork.OutputNodes[1].CurrentValue / 100);
        }

        internal override AbstractBrain Replicate()
        {
            var newBrain = new NeuralBrain();
            newBrain._adrenalineModeNetwork = _adrenalineModeNetwork.Replicate();
            newBrain._foragerModeNetwork = _foragerModeNetwork.Replicate();

            newBrain.ConnectReinforcementNodes();

            return newBrain;
        }

        internal override void Mutate()
        {
            Console.WriteLine("AdrenalineMode: ");
            _adrenalineModeNetwork.Mutate();
            Console.WriteLine("NeutralMode: ");
            _foragerModeNetwork.Mutate();
        }
    }
}
