using DawnOnline.Simulation.Collision;

namespace DawnOnline.Simulation
{
    public interface IForm
    {
        double BoundingCircleRadius { get; }

        IPolygon Shape { get; }
    }
}
