using DawnOnline.Simulation.Collision;

namespace DawnOnline.Simulation
{
    internal class Movement : IMovement
    {
        public Vector ForwardMotion { get; set; }
        public double TurnMotion { get; set; }
        public double FatigueCost { get; set; }
    }
}
