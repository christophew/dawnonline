namespace DawnOnline.Simulation.Statistics
{
    public class Monitor
    {
        private int _maxAmount = 100;
        private int _current;
        private int _criticalThreshold = 80;

        internal Monitor()
        {
        }

        internal Monitor(int max)
        {
            _maxAmount = max;
            _criticalThreshold = max*80/100;
        }

        internal bool CanIncrease(int amount)
        {
            return _current + amount < _maxAmount;
        }

        internal void Increase(int amount)
        {
            _current += amount;

            if (_current > _maxAmount)
                _current = _maxAmount;
        }

        internal void Decrease(int amount)
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

        public double PercentFilled
        {
            get
            {
                return _current*100.0/_maxAmount;
            }
        }

        public bool IsFilled
        {
            get
            {
                return _current == _maxAmount;   
            }
        }

        internal void Clear()
        {
            _current = 0;
        }
    }
}
