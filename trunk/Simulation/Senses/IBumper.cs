using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOnline.Simulation.Senses
{
    public interface IBumper
    {
        bool Hit { get; }

        void Clear();
    }
}
