using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DawnOnline.Simulation.Statistics
{
    internal class Monitor
    {
        private static int _maxAmount = 100;
        private int _current;


        internal bool CanIncrease(int amount)
        {
            return _current + amount < _maxAmount;
        }

        internal void Increase(int amount)
        {
            _current += amount;

            Debug.Assert(_current < _maxAmount);
        }

        internal void Decrease(int amount)
        {
            _current -= amount;

            if (_current < 0)
                _current = 0;
        }
    }
}
