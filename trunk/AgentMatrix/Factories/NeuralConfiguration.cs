using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.AgentMatrix.Brains.Neural;

namespace DawnOnline.AgentMatrix.Factories
{
    public static class NeuralConfiguration
    {
        internal static Node.NodeTypeEnum NodeType = Node.NodeTypeEnum.Linear;

        public static void UseSigmoidNodes()
        {
            NodeType = Node.NodeTypeEnum.Signoid;
        }
    }
}
