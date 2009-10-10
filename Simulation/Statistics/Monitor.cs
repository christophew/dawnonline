using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DawnOnline.Simulation.Statistics
{
    internal class Monitor
    {
        private int _maxAmount = 100;
        private int _current;
        private int _criticalThreshold = 80;

        public Monitor()
        {
        }

        public Monitor(int max)
        {
            _maxAmount = max;
            _criticalThreshold = max*80/100;
        }

        public bool CanIncrease(int amount)
        {
            return _current + amount < _maxAmount;
        }

        public void Increase(int amount)
        {
            _current += amount;

            if (_current > _maxAmount)
                _current = _maxAmount;
        }

        public void Decrease(int amount)
        {
            _current -= amount;

            if (_current < 0)
                _current = 0;
        }

        public bool IsCritical
        {
            get
            {
                return _current > _criticalThreshold;
            }
        }
    }
}
