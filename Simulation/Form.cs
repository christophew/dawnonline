using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOnline.Simulation
{
    class Form : IForm
    {
        #region IForm Members

        private readonly Guid _id = Guid.NewGuid();

        public Guid Id
        {
            get { return _id; }
        }

        public double Radius
        {
            get; set;
        }

        #endregion
    }
}
