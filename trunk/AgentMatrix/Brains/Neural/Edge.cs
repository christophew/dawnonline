﻿using System.IO;

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

        internal void Serialize(BinaryWriter writer)
        {
            writer.Write(_multiplier);

            // ToNode is static on create
        }

        internal void Deserialize(BinaryReader reader)
        {
            _multiplier = reader.ReadDouble();
        }
    }
}
