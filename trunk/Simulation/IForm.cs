using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulation
{
    public interface IForm
    {
        Guid Id { get; }
        double Radius { get; }
    }
}
