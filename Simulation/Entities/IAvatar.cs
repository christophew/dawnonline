using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOnline.Simulation.Entities
{
    public interface IAvatar : ICreature
    {
        void TurnLeft();
        void TurnRight();
        void TurnLeftSlow();
        void TurnRightSlow();
        void StrafeLeft();
        void StrafeRight();
        void RunForward();
        void WalkForward();
        void WalkBackward();

        void Fire();
        void FireRocket();

        void BuildEntity(EntityType entityType);
    }
}
