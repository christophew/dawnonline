using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnClient
{
    public enum AvatarCommand
    {
        Unknown = 0,
        TurnRight = 1,
        TurnLeft = 2,
        TurnRightSlow = 3,
        TurnLeftSlow = 4,
        RunForward = 5,
        WalkForward = 6,
        WalkBackward = 7,
        StrafeRight = 8,
        StrafeLeft = 9,
        Fire = 10,
        FireRocket = 11
    }
}
