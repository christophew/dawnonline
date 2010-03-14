using DawnOnline.Simulation.Collision;
namespace DawnOnline.Simulation
{
    public interface IActionQueue
    {
        Vector ForwardMotion { get; }
        double TurnMotion { get; }
        double FatigueCost { get; }

        double Damage { get; }
        bool HasAttacked { get; }
    }
}
