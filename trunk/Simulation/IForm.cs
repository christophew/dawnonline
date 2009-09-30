using DawnOnline.Simulation.Collision;

namespace DawnOnline.Simulation
{
    public interface IForm
    {
        double Radius { get; }

        IPolygon Shape { get; }
    }
}
