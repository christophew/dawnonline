using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Entities;
using DawnOnline.Simulation.Senses;
using Microsoft.Xna.Framework;

namespace DawnOnline.Simulation.Builders
{
    public static class SensorBuilder
    {
        public static IBumper CreateBumper(ICreature creature, Vector2 offset)
        {
            var myCreature = creature as Creature;
            Debug.Assert(myCreature != null);

            return new Bumper(myCreature, offset);
        }

        public static IEye CreateEye(ICreature creature, double angle, double visionAngle, double visionDistance)
        {
            var myCreature = creature as Creature;
            Debug.Assert(myCreature != null);

            var eye = new Eye(myCreature);

            eye.Angle = angle;
            eye.VisionDistance = visionDistance;
            eye.VisionAngleDelta = visionAngle;

            return eye;
        }

        public static IEye CreateNose(ICreature creature, double angle, double sniffAngle, double sniffDistance)
        {
            return CreateEye(creature, angle, sniffAngle, sniffDistance);
        }

        public static IEar CreateEar(ICreature creature, Vector2 offset)
        {
            var myCreature = creature as Creature;
            Debug.Assert(myCreature != null);

            return new Ear(myCreature, offset);
        }

    }
}
