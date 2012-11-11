namespace SharedConstants
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
        FireRocket = 11,
        RunBackward = 12,

        Attack = 13,

        // Specific for creature matrix control
        ForwardMotion = 14,
        TurnMotion = 14,
    }
}
