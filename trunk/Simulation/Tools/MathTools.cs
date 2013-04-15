using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DawnOnline.Simulation.Tools
{
    static public class MathTools
    {
        public static double GetDistance2(Vector2 position1, Vector2 position2)
        {
            double deltaX = position1.X - position2.X;
            double deltaY = position1.Y - position2.Y;

            double distance2 = deltaX * deltaX + deltaY * deltaY;
            return distance2;
        }

        public static double GetDistance(Vector2 position1, Vector2 position2)
        {
            return Math.Sqrt(GetDistance2(position1, position2));
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
                if (pyRes == 0.0)
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

        public static Vector2 OffsetCoordinate(Vector2 origin, double angle, double distance)
        {
            return new Vector2((float)(origin.X + Math.Cos(angle)*distance), (float)(origin.Y + Math.Sin(angle)*distance));
        }

        public static bool CirclesIntersect(Vector2 position1, double radius1, Vector2 position2, double radius2)
        {
            double radius = radius1 + radius2;
            return GetDistance2(position1, position2) < radius*radius;
        }
    }
}
