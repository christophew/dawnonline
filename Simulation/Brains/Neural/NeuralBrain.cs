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

            _neutralModeNetwork.InputNodes[0].OutGoingEdges[0].Multiplier = 1.5;
            _neutralModeNetwork.InputNodes[1].OutGoingEdges[1].Multiplier = 1.5;
            _neutralModeNetwork.InputNodes[2].OutGoingEdges[2].Multiplier = 1.5;

            _neutralModeNetwork.LayerNodes[0].OutGoingEdges[0].Multiplier = -1.5;
            _neutralModeNetwork.LayerNodes[1].OutGoingEdges[1].Multiplier = 1.5;
            _neutralModeNetwork.LayerNodes[2].OutGoingEdges[0].Multiplier = 1.5;
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
            var leftEyeCheck = _eyeSee[_leftEye] < 0 ? 0 : 100.0 * (_leftEye.VisionDistance - _eyeSee[_leftEye]) / _leftEye.VisionDistance;
            var forwardEyeCheck = _eyeSee[_forwardEye] < 0 ? 0 : 100.0 * (_forwardEye.VisionDistance - _eyeSee[_forwardEye]) / _forwardEye.VisionDistance;
            var rightEyeCheck = _eyeSee[_rightEye] < 0 ? 0 : 100.0 * (_rightEye.VisionDistance - _eyeSee[_rightEye]) / _rightEye.VisionDistance;

            _neutralModeNetwork.InputNodes[0].CurrentValue = leftEyeCheck;
            _neutralModeNetwork.InputNodes[1].CurrentValue = forwardEyeCheck;
            _neutralModeNetwork.InputNodes[2].CurrentValue = rightEyeCheck;

            // Process
            _neutralModeNetwork.Propagate();

            // Feed output to creature
            MyCreature.Turn(_neutralModeNetwork.OutputNodes[0].CurrentValue / 100);
            MyCreature.Thrust(_neutralModeNetwork.OutputNodes[1].CurrentValue / 100);
        }

        internal override AbstractBrain Replicate()
        {
            var newBrain = new NeuralBrain();
            newBrain._neutralModeNetwork = _neutralModeNetwork.Replicate();

            return newBrain;
        }

        internal override void Mutate()
        {
            _neutralModeNetwork.Mutate();
        }
    }
}
