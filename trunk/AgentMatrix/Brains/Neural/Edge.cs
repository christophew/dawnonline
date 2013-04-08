namespace DawnOnline.AgentMatrix.Brains.Neural
{
    class Edge
    {
        //internal Edge()
        //{
        //    Multiplier = 0;
        //}

        private double _multiplier;
        internal double Multiplier
        {
            get { return _multiplier; }
            set
            {
                _multiplier = value;

                // [-2, 2]
                if (_multiplier > 2)
                    _multiplier = 2;
                if (_multiplier < -2)
                    _multiplier = -2;
            }
        }


        internal Node ToNode { get; set; }
    }
}
