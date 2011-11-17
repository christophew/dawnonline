using System.Diagnostics;
using System;
using DawnOnline.Simulation.Entities;
using DawnOnline.Simulation.Senses;
using DawnOnline.Simulation.Tools;

namespace DawnOnline.Simulation.Brains
{
    internal class ForagerBrain : PredatorBrain
    {
        protected override void NeutralState(TimeSpan timeDelta)
        {
            // Forage
            if (_forwardEye.SeesAnObstacle(EntityType.Treasure))
            {
                MyCreature.RunForward();
            }
            if (_leftEye.SeesAnObstacle(EntityType.Treasure))
            {
                MyCreature.TurnLeft();
                return;
            }
            if (_rightEye.SeesAnObstacle(EntityType.Treasure))
            {
                MyCreature.TurnRight();
                return;
            }

            // Turn on bumper-hit
            if (_forwardBumper.Hit)
            {
                if (_evading == EvadeState.None)
                {
                    // Chose left or right
                    _evading = Globals.Radomizer.Next(2) == 0 ? EvadeState.Left : EvadeState.Right;
                    _startEvading = DateTime.Now;
                }

                if (_evading == EvadeState.Left)
                    MyCreature.TurnLeft();
                if (_evading == EvadeState.Right)
                    MyCreature.TurnRight();
                return;
            }

            // Reset evade direction x-seconds after evade
            if (_evading != EvadeState.None)
            {
                if ((DateTime.Now - _startEvading).TotalSeconds > 5)
                    _evading = EvadeState.None;
            }

            DoRandomAction(1000);
        }
    }
}
