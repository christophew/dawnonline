using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Messaging;
using DawnOnline.AgentMatrix.Factories;

namespace DawnOnline.AgentMatrix.Brains.Neural
{
    class Node
    {
        internal enum NodeTypeEnum
        {
            Unknown,
            Linear,
            Sigmoid,
        }

        // Bias
        private int _threshold = 0;
        internal int Threshold
        {
            get { return _threshold; }
            set
            {
                _threshold = value;

                // [-100, 100]
                if (_threshold > 100)
                    _threshold = 100;
                if (_threshold < -100)
                    _threshold = -100;
            }
        }

        internal Edge[] OutGoingEdges { get; set; }

        private double _currentValue = 0.0;

        internal void SetValue(double value)
        {
            _currentValue = value;
        }

        internal void AddValue(double delta)
        {
            _currentValue += delta;
        }

        internal void ClearValue()
        {
            _currentValue = 0.0;
        }

        internal double GetValue()
        {
            if (NeuralConfiguration.NodeType == NodeTypeEnum.Linear)
                return GetLinearValue();
            if (NeuralConfiguration.NodeType == NodeTypeEnum.Sigmoid)
                return GetSigmoidValue();

            throw new NotImplementedException();
        }

        private double GetLinearValue()
        {
            var normalizedValue = _currentValue;

            // [-100, 100]
            if (normalizedValue > 100)
                normalizedValue = 100;
            if (normalizedValue < -100)
                normalizedValue = -100;

            return normalizedValue;
        }

        private double GetSigmoidValue()
        {
            var scaledCurrent = ((double)_currentValue) / NeuralConfiguration.SigmoidScale;         // [-100, 100] => input
            var sigmoid = LogisticFunction(scaledCurrent);                                          // input => [0, 1] 
            var result = (sigmoid - 0.5) * 2.0 * 100.0;                         // [0, 1] => [-0.5, 0.5] => [-1, 1] => [-100, 100]

            // Validate (should no longer be possible)
            if (result > 100)
                throw new InvalidOperationException("value outofbound: should no longer be possible");
            if (result < -100)
                throw new InvalidOperationException("value outofbound: should no longer be possible");

            return result;
        }

        private static double LogisticFunction(double val)
        {
            return (1.0 / (1.0 + Math.Exp(-(val * NeuralConfiguration.SigmoidSlope))));
        } 

        internal void Propagate(int error)
        {
            var currentValue = GetValue();

            // For fuzzy network
            if (error != 0)
            {
                var fault = Globals.Radomizer.Next(error);
                currentValue += fault - error/2;
            }

            // Use Abs threshold to handle possible negative values => otherwise we would favour the positive values
            // TO VERIFY: threshold or bias? (bias would be deducted from value instead)
            if (Math.Abs(currentValue) < Threshold)
                return;

            foreach (var edge in OutGoingEdges)
            {
                if (edge.Enabled)
                {
                    edge.ToNode.AddValue(currentValue * edge.Multiplier);
                }
            }
        }

        internal void Serialize(BinaryWriter writer)
        {
            writer.Write(_threshold);

            if (OutGoingEdges != null)
            {
                writer.Write(OutGoingEdges.Length);
                foreach (var edge in OutGoingEdges)
                {
                    edge.Serialize(writer);
                }
            }
            else
            {
                writer.Write(0);
            }
        }

        internal void Deserialize(BinaryReader reader)
        {
            _threshold = reader.ReadInt32();

            var nrOfEdges = reader.ReadInt32();
            if (OutGoingEdges != null)
            {
                Debug.Assert(nrOfEdges == OutGoingEdges.Length, "OutGoingEdges should have been initialized");
                foreach (var edge in OutGoingEdges)
                {
                    edge.Deserialize(reader);
                }
            }
            else
            {
                Debug.Assert(nrOfEdges == 0, "OutGoingEdges should have been initialized");
            }
        }

    }
}
