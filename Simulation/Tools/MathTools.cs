using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOnline.Simulation.Tools
{
    static class MathTools
    {
        public static double GetDistance2(Coordinate position1, Coordinate position2)
        {
            double deltaX = position1.X - position2.X;
            double deltaY = position1.Y - position2.Y;

            double distance2 = deltaX * deltaX + deltaY * deltaY;
            return distance2;
        }

        public static double GetAngle(double px1, double py1, double px2, double py2)
        {
            // Negate X and Y values
            double pxRes = px2 - px1;
            double pyRes = py2 - py1;
            double angle = 0.0;
            // Calculate the angle
            if (pxRes == 0.0)
            {
                if (pxRes == 0.0)
                    angle = 0.0;
                else if (pyRes > 0.0) angle = System.Math.PI / 2.0;
                else
                    angle = System.Math.PI * 3.0 / 2.0;
            }
            else if (pyRes == 0.0)
            {
                if (pxRes > 0.0)
                    angle = 0.0;
                else
                    angle = System.Math.PI;
            }
            else
            {
                if (pxRes < 0.0)
                    angle = System.Math.Atan(pyRes / pxRes) + System.Math.PI;
                else if (pyRes < 0.0) angle = System.Math.Atan(pyRes / pxRes) + (2 * System.Math.PI);
                else
                    angle = System.Math.Atan(pyRes / pxRes);
            }
            // Convert to degrees
            //angle = angle*180/System.Math.PI;
            return angle;

        }

        /// <summary>
        /// convert angle to the range [0..2pi[
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static double NormalizeAngle(double angle)
        {
            if (angle > 0.0)
            {
                while (angle > Math.PI * 2.0) angle -= Math.PI*2.0;
            }
            else
            {
                while (angle < 0.0) angle += Math.PI * 2.0;
            }

            return angle;
        }

        public static double ConvertToRadials(double degrees)
        {
            return degrees*Math.PI/180.0;
        }

        public static Coordinate OffsetCoordinate(Coordinate origin, double angle, double distance)
        {
            return new Coordinate(origin.X + Math.Cos(angle)*distance, origin.Y + Math.Sin(angle)*distance);
        }
    }
}
