using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOnline.Simulation
{
    public interface IForm
    {
        Guid Id { get; }
        double Radius { get; }
    }
}
