using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Entities;
using SharedConstants;

namespace DawnPhotonApp
{
    static class ApplyCreatureCommand
    {
        internal static void ApplyCommand(IAvatar creature, AvatarCommand command)
        {
            switch (command)
            {
                case AvatarCommand.RunForward:
                    creature.RunForward();
                    break;
                case AvatarCommand.RunBackward:
                    creature.RunBackward();
                    break;
                case AvatarCommand.WalkForward:
                    creature.WalkForward();
                    break;
                case AvatarCommand.WalkBackward:
                    creature.WalkBackward();
                    break;
                case AvatarCommand.TurnLeft:
                    creature.TurnLeft();
                    break;
                case AvatarCommand.TurnRight:
                    creature.TurnRight();
                    break;
                case AvatarCommand.StrafeLeft:
                    creature.StrafeLeft();
                    break;
                case AvatarCommand.StrafeRight:
                    creature.StrafeRight();
                    break;
                case AvatarCommand.TurnLeftSlow:
                    creature.TurnLeftSlow();
                    break;
                case AvatarCommand.TurnRightSlow:
                    creature.TurnRightSlow();
                    break;
                case AvatarCommand.Attack:
                    creature.Attack();
                    break;
                case AvatarCommand.Fire:
                    creature.Fire();
                    break;
                case AvatarCommand.FireRocket:
                    creature.FireRocket();
                    break;
                default:
                    ApplyPrecisionCommand(creature, command);
                    break;
            }
        }

        private static void ApplyPrecisionCommand(IAvatar creature, AvatarCommand command)
        {
            switch (command)
            {
                case AvatarCommand.Forward10: creature.Thrust(0.1); break;
                case AvatarCommand.Forward20: creature.Thrust(0.2); break;
                case AvatarCommand.Forward30: creature.Thrust(0.3); break;
                case AvatarCommand.Forward40: creature.Thrust(0.4); break;
                case AvatarCommand.Forward50: creature.Thrust(0.5); break;
                case AvatarCommand.Forward60: creature.Thrust(0.6); break;
                case AvatarCommand.Forward70: creature.Thrust(0.7); break;
                case AvatarCommand.Forward80: creature.Thrust(0.8); break;
                case AvatarCommand.Forward90: creature.Thrust(0.9); break;
                case AvatarCommand.Forward100: creature.Thrust(1.0); break;

                case AvatarCommand.Backward10: creature.Thrust(-0.1); break;
                case AvatarCommand.Backward20: creature.Thrust(-0.2); break;
                case AvatarCommand.Backward30: creature.Thrust(-0.3); break;
                case AvatarCommand.Backward40: creature.Thrust(-0.4); break;
                case AvatarCommand.Backward50: creature.Thrust(-0.5); break;
                case AvatarCommand.Backward60: creature.Thrust(-0.6); break;
                case AvatarCommand.Backward70: creature.Thrust(-0.7); break;
                case AvatarCommand.Backward80: creature.Thrust(-0.8); break;
                case AvatarCommand.Backward90: creature.Thrust(-0.9); break;
                case AvatarCommand.Backward100: creature.Thrust(-1.0); break;

                case AvatarCommand.Right10: creature.Turn(0.1); break;
                case AvatarCommand.Right20: creature.Turn(0.2); break;
                case AvatarCommand.Right30: creature.Turn(0.3); break;
                case AvatarCommand.Right40: creature.Turn(0.4); break;
                case AvatarCommand.Right50: creature.Turn(0.5); break;
                case AvatarCommand.Right60: creature.Turn(0.6); break;
                case AvatarCommand.Right70: creature.Turn(0.7); break;
                case AvatarCommand.Right80: creature.Turn(0.8); break;
                case AvatarCommand.Right90: creature.Turn(0.9); break;
                case AvatarCommand.Right100: creature.Turn(1.0); break;

                case AvatarCommand.Left10: creature.Turn(-0.1); break;
                case AvatarCommand.Left20: creature.Turn(-0.2); break;
                case AvatarCommand.Left30: creature.Turn(-0.3); break;
                case AvatarCommand.Left40: creature.Turn(-0.4); break;
                case AvatarCommand.Left50: creature.Turn(-0.5); break;
                case AvatarCommand.Left60: creature.Turn(-0.6); break;
                case AvatarCommand.Left70: creature.Turn(-0.7); break;
                case AvatarCommand.Left80: creature.Turn(-0.8); break;
                case AvatarCommand.Left90: creature.Turn(-0.9); break;
                case AvatarCommand.Left100: creature.Turn(-1.0); break;
            }
        }

    }
}
