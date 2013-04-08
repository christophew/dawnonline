using System;

namespace DawnOnline.AgentMatrix.Brains.Neural
{
    class Node
    {
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
                _currentValue = value;

                // [-100, 100]
                if (_currentValue > 100)
                    _currentValue = 100;
                if (_currentValue < -100)
                    _currentValue = -100;
            }
        }

        internal void Propagate()
        {
            if (Math.Abs(CurrentValue) < Threshold)
                return;

            foreach (var edge in OutGoingEdges)
            {
                edge.ToNode.CurrentValue += CurrentValue * edge.Multiplier;
            }
        }
    }
}
