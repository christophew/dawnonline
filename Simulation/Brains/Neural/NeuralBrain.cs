using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Brains;

namespace DawnOnline.Simulation.Brains.Neural
{
    internal class NeuralBrain : PredatorBrain
    {
        private NeuralNetwork _neutralModeNetwork;

        internal NeuralBrain()
        {
            _neutralModeNetwork = new NeuralNetwork(3, 3, 2);

            _neutralModeNetwork.InputNodes[0].OutGoingEdges[0].Multiplier = 1;
            _neutralModeNetwork.InputNodes[1].OutGoingEdges[1].Multiplier = 1;
            _neutralModeNetwork.InputNodes[2].OutGoingEdges[2].Multiplier = 1;

            _neutralModeNetwork.LayerNodes[0].OutGoingEdges[0].Multiplier = -1;
            _neutralModeNetwork.LayerNodes[1].OutGoingEdges[1].Multiplier = 1;
            _neutralModeNetwork.LayerNodes[2].OutGoingEdges[0].Multiplier = 1;
        }

        internal override void ClearState()
        {
            base.ClearState();
            _neutralModeNetwork.Clear();
        }

        protected override void AdrenalineState()
        {
            // Find something to attack
            var creaturesToAttack = MyCreature.FindCreatureToAttack(MyCreature.FoodSpecies);
            if (creaturesToAttack != null)
            {
                MyCreature.Attack(creaturesToAttack);
            }

            // Init network with input values
            var leftEyeCheck = _leftEye.SeesACreature(MyCreature.FoodSpecies, MyCreature.SpawnPoint);
            var forwardEyeCheck = _forwardEye.SeesACreature(MyCreature.FoodSpecies, MyCreature.SpawnPoint);
            var rightEyeCheck = _rightEye.SeesACreature(MyCreature.FoodSpecies, MyCreature.SpawnPoint);

            _neutralModeNetwork.InputNodes[0].CurrentValue = leftEyeCheck ? 10 : 0;
            _neutralModeNetwork.InputNodes[1].CurrentValue = forwardEyeCheck ? 10 : 0;
            _neutralModeNetwork.InputNodes[2].CurrentValue = rightEyeCheck ? 10 : 0;

            // Process
            _neutralModeNetwork.Propagate();

            // Feed output to creature
            if (_neutralModeNetwork.OutputNodes[0].CurrentValue > 0)
            {
                MyCreature.TurnRight();
            }
            if (_neutralModeNetwork.OutputNodes[0].CurrentValue < 0)
            {
                MyCreature.TurnLeft();
            }
            if (_neutralModeNetwork.OutputNodes[1].CurrentValue > 0)
            {
                MyCreature.RunForward();
            }
        }

        internal override AbstractBrain Replicate()
        {
            var newBrain = new NeuralBrain();
            newBrain._neutralModeNetwork = _neutralModeNetwork.Replicate();

            // TODO: MUTATE

            return newBrain;
        }
    }
}
