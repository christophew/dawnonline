namespace DawnOnline.Simulation.Brains.Neural
{
    class Node
    {
        internal int Threshold { get; set; }
        internal Edge[] OutGoingEdges { get; set; }

        internal double CurrentValue { get; set; }

        internal void Propagate()
        {
            if (CurrentValue < Threshold)
                return;

            foreach (var edge in OutGoingEdges)
            {
                edge.ToNode.CurrentValue += CurrentValue * edge.Multiplier;

                // [-100, 100]
                if (edge.ToNode.CurrentValue > 100)
                    edge.ToNode.CurrentValue = 100;
                if (edge.ToNode.CurrentValue < -100)
                    edge.ToNode.CurrentValue = -100;
            }
        }
    }
}
