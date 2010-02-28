namespace DawnOnline.Simulation
{
    public interface IMovement
    {
        bool MoveForward { get; }
        bool TurnLeft { get; }
        bool TurnRight { get; }
    }
}
