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
            if (Math.Abs(CurrentValue) >= Threshold)
                CurrentValue = NodeToReinforce.CurrentValue;
        }
    }
}
