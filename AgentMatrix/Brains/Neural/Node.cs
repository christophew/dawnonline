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
            Signoid
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
            // TODO: verify use of tanh instead of sigmoid

            if (NeuralConfiguration.NodeType == NodeTypeEnum.Linear)
                return GetLinearValue();
            if (NeuralConfiguration.NodeType == NodeTypeEnum.Signoid)
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
            var scaledCurrent = ((double)_currentValue) / 25.0;         // [-100, 100] => [-1, 1]
            var sigmoid = LogisticFunction(scaledCurrent);              // [-1, 1] => [0, 1] 
            var result = (sigmoid - 0.5) * 100.0 * 2.0;              // [0, 1] => [-0.5, 0.5] => [-1, 1] => [-100, 100]

            // Validate (should no longer be possible)
            if (result > 100)
                throw new InvalidOperationException("value outofbound: should no longer be possible");
            if (result < -100)
                throw new InvalidOperationException("value outofbound: should no longer be possible");

            return result;
        }

        private static double LogisticFunction(double val)
        {
            return (1.0 / (1.0 + System.Math.Exp(-val)));
        } 

        internal void Propagate()
        {
            var currentValue = GetValue();

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
