using DawnOnline.Simulation.Statistics;

namespace DawnOnline.Simulation
{
    public enum CreatureType
    {
        Unknown,
        Avatar,
        Predator,
        Rabbit,
        Plant
    }

    public interface ICreature
    {
        CreatureType Specy { get; }

        bool Alive { get; }

        bool IsDying { get; }
        bool IsTired { get; }

        void Rest();
        void WalkForward(); 
        void RunForward();
        void TurnLeft();
        void TurnRight();
        void Attack();

        void Move();
        void ClearActionQueue();
        void ApplyActionQueue(double timeDelta);

        IPlacement Place { get; }
        ICharacterSheet iCharacterSheet { get; }
    }
}
