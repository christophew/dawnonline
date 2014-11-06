using System;
using System.Diagnostics;
using System.IO;

namespace DawnOnline.AgentMatrix.Brains.Neural
{
    class Node
    {
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
        internal double CurrentValue
        {
            get { return _currentValue; }
            set
            {
                // TODO: verify use of tanh instead of sigmoid


                var scaledCurrent = ((double)value) / 25.0;         // [-100, 100] => [-4, 4]
                var sigmoid = LogisticFunction(scaledCurrent);      // [-4, 4] => [0, 1] 
                _currentValue = (sigmoid - 0.5) * 100.0 * 2.0;      // [0, 1] => [-0.5, 0.5] => [-1, 1] => [-100, 100]

                // Validate (should no longer be possible)
                if (_currentValue > 100)
                    throw new InvalidOperationException("value outofbound: should no longer be possible");
                if (_currentValue < -100)
                    throw new InvalidOperationException("value outofbound: should no longer be possible");

                // [-100, 100]
                //if (_currentValue > 100)
                //    _currentValue = 100;
                //if (_currentValue < -100)
                //    _currentValue = -100;
            }
        }

        private static double LogisticFunction(double val)
        {
            return (1.0 / (1.0 + System.Math.Exp(-val)));
        } 

        internal void Propagate()
        {
            if (Math.Abs(CurrentValue) < Threshold)
                return;

            foreach (var edge in OutGoingEdges)
            {
                if (edge.Enabled)
                {
                    edge.ToNode.CurrentValue += CurrentValue * edge.Multiplier;
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
