using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DawnOnline.AgentMatrix.Brains.Neural
{
    class MemoryInputNode : Node
    {
        private TimeSpan _rememberTime;

        internal Node NodeToRemember { get; set; }

        internal MemoryInputNode(TimeSpan rememberTime)
        {
            _rememberTime = rememberTime;
        }

        internal void AddRememberedValue(double rememberedValue, TimeSpan timeDelta)
        {
            Debug.Assert(_rememberTime.TotalMilliseconds != 0);
            var newValue = CurrentValue
                           - (CurrentValue*timeDelta.TotalMilliseconds/_rememberTime.TotalMilliseconds)
                           + (rememberedValue*timeDelta.TotalMilliseconds/_rememberTime.TotalMilliseconds);
            CurrentValue = newValue;
        }

        internal void Remember(TimeSpan timeDelta)
        {
            Debug.Assert(NodeToRemember != null);
            AddRememberedValue(NodeToRemember.CurrentValue, timeDelta);
        }
    }
}
