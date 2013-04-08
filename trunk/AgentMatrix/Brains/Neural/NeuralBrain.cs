﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Brains;
using DawnOnline.Simulation.Entities;
using DawnOnline.Simulation.Senses;

namespace DawnOnline.AgentMatrix.Brains.Neural
{
    internal class NeuralBrain : ForagerBrain
    {
        private NeuralNetwork _adrenalineModeNetwork;
        private int _adrenalineInputNodes = 18; // 3x eye x3, bumper, health, stamina, 2x random, 2x ears x2
        private int _adrenalineOutputNodes = 4; 
        private NeuralNetwork _foragerModeNetwork;
        private int _foragerInputNodes = 18; // 3x eye x3, bumper, health, stamina, 2x random, 2x ears x2
        private int _foragerOutputNodes = 4;

        internal void PredefineRandomBehaviour()
        {
            _adrenalineModeNetwork = new NeuralNetwork(_adrenalineInputNodes, _adrenalineInputNodes * 2, _adrenalineOutputNodes + _adrenalineInputNodes, _adrenalineInputNodes);
            _foragerModeNetwork = new NeuralNetwork(_foragerInputNodes, _foragerInputNodes * 2, _foragerOutputNodes + _foragerInputNodes, _foragerInputNodes);

            RandomizeNetwork(_adrenalineModeNetwork);
            RandomizeNetwork(_foragerModeNetwork);
        }

        private static void RandomizeNetwork(NeuralNetwork network)
        {
            foreach (var node in network.InputNodes)
            {
                node.Threshold = Globals.Radomizer.Next(200) - 100;

                foreach (var edge in node.OutGoingEdges)
                {
                    edge.Multiplier = Globals.Radomizer.Next(200) - 100;
                }
            }

            foreach (var node in network.LayerNodes)
            {
                node.Threshold = Globals.Radomizer.Next(200) - 100;

                foreach (var edge in node.OutGoingEdges)
                {
                    edge.Multiplier = Globals.Radomizer.Next(200) - 100;
                }
            }

            foreach (var node in network.ReinforcementInputNodes)
            {
                node.Threshold = Globals.Radomizer.Next(200) - 100;

                foreach (var edge in node.OutGoingEdges)
                {
                    edge.Multiplier = Globals.Radomizer.Next(200) - 100;
                }
            }
        }

        internal void PredefineBehaviour()
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
                _foragerModeNetwork.InputNodes[3].OutGoingEdges[0].Multiplier = 1;
                _foragerModeNetwork.InputNodes[4].OutGoingEdges[1].Multiplier = 1;
                _foragerModeNetwork.InputNodes[5].OutGoingEdges[2].Multiplier = 1;

                _foragerModeNetwork.LayerNodes[0].OutGoingEdges[0].Multiplier = -1;
                _foragerModeNetwork.LayerNodes[1].OutGoingEdges[1].Multiplier = 1;
                _foragerModeNetwork.LayerNodes[2].OutGoingEdges[0].Multiplier = 1;


                // Reinforcement
                BuildStandardReinforcementSetup(_foragerModeNetwork);


                // bumper
                _foragerModeNetwork.ReinforcementInputNodes[9].OutGoingEdges[0].Multiplier = 0.5;
                _foragerModeNetwork.ReinforcementInputNodes[9].OutGoingEdges[1].Multiplier = -2; 
                _foragerModeNetwork.ReinforcementInputNodes[9].OutGoingEdges[2].Multiplier = -0.5; 

                // random nodes

                // reinforced random: input nodes
                _foragerModeNetwork.ReinforcementInputNodes[10].OutGoingEdges[0].Multiplier = .1;
                _foragerModeNetwork.ReinforcementInputNodes[10].OutGoingEdges[1].Multiplier = .3;
                _foragerModeNetwork.ReinforcementInputNodes[10].OutGoingEdges[2].Multiplier = -.1;

                _foragerModeNetwork.ReinforcementInputNodes[11].OutGoingEdges[0].Multiplier = -.1;
                _foragerModeNetwork.ReinforcementInputNodes[11].OutGoingEdges[1].Multiplier = .3;
                _foragerModeNetwork.ReinforcementInputNodes[11].OutGoingEdges[2].Multiplier = .1;
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

        public override void ClearState()
        {
            base.ClearState();

            _adrenalineModeNetwork.Reset();
            _foragerModeNetwork.Reset();
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
            network.InputNodes[i++].CurrentValue = _forwardBumper.Hit ? 100 : 0;
            network.InputNodes[i++].CurrentValue = Globals.Radomizer.Next(50);
            network.InputNodes[i++].CurrentValue = Globals.Radomizer.Next(50);
            network.InputNodes[i++].CurrentValue = this.MyCreature.CharacterSheet.Damage.PercentFilled;
            network.InputNodes[i++].CurrentValue = this.MyCreature.CharacterSheet.Fatigue.PercentFilled;
            Debug.Assert(i == 14);

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

        protected override void NeutralState(TimeSpan timeDelta)
        {
            RunNetwork(_foragerModeNetwork, timeDelta);
        }

        protected override void AdrenalineState(TimeSpan timeDelta)
        {
            // TODO: attack should also be an output state of the neuralnetwork
            // Find something to attack
            var creatureToAttack = MyCreature.FindCreatureToAttack(MyCreature.FoodSpecies);
            if (creatureToAttack != null)
            {
                MyCreature.Attack();
            }

            RunNetwork(_adrenalineModeNetwork, timeDelta);
        }

        public override IBrain Replicate(IBrain mate)
        {
            // TODO: optimize
            // 'new' will also create a useless intial network

            var newBrain = new NeuralBrain();
            newBrain._adrenalineModeNetwork = _adrenalineModeNetwork.Replicate();
            //newBrain._foragerModeNetwork = _foragerModeNetwork.Replicate();

            // crossover
            var neuralMate = mate as NeuralBrain;
            Debug.Assert(neuralMate != null, "sodomy!");
            newBrain._foragerModeNetwork = neuralMate._foragerModeNetwork.Replicate();

            newBrain.ConnectReinforcementNodes();

            return newBrain;
        }

        public override void Mutate()
        {
            Console.WriteLine("AdrenalineMode: ");
            _adrenalineModeNetwork.Mutate();
            Console.WriteLine("NeutralMode: ");
            _foragerModeNetwork.Mutate();
        }
    }
}