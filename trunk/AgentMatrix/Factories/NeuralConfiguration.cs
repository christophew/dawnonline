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
        internal static double SigmoidSlope = 1.0;
        internal static double SigmoidScale = 25.0;

        public static void UseSigmoidNodes()
        {
            NodeType = Node.NodeTypeEnum.Sigmoid;
        }
    }
}
