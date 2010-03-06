using DawnOnline.Simulation.Collision;
namespace DawnOnline.Simulation
{
    public interface IMovement
    {
        Vector ForwardMotion { get; }
        double TurnMotion { get; }
        double FatigueCost { get; }
    }
}
