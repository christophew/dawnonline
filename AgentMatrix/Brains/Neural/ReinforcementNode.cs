using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOnline.AgentMatrix.Brains.Neural
{
    class ReinforcementNode : Node
    {
        internal Node NodeToReinforce { get; set; }

        internal void Reinforce()
        {
            // TO VERIFY: this threshpmd check was pretty useless => threshold of receiving node was checked, instead of sending node
            // => do we need a threshold check here?
            //if (Math.Abs(CurrentValue) >= Threshold)


            SetValue(NodeToReinforce.GetValue());
        }
    }
}
