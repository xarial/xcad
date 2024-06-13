//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Utils;

namespace Xarial.XCad.Geometry.Structures
{
    /// <summary>
    /// Additional methods for <see cref="Axis"/>
    /// </summary>
    public static class AxisExtension
    {
        /// <summary>
        /// Determines if this axis is collinear to other axis
        /// </summary>
        /// <param name="axis">This axis</param>
        /// <param name="otherAxis">Other axis</param>
        /// <param name="angTol">Angle tolerance</param>
        /// <param name="lengthTol">Length tolerance</param>
        /// <returns>True if collinear, false if not</returns>
        public static bool IsCollinear(this Axis axis, Axis otherAxis, double angTol = Numeric.DEFAULT_ANGLE_TOLERANCE,
            double lengthTol = Numeric.DEFAULT_LENGTH_TOLERANCE) 
        {
            if (axis.Direction.IsParallel(otherAxis.Direction, angTol)) 
            {
                var altDir = axis.Point - otherAxis.Point;

                if (altDir.GetLength() < lengthTol)
                {
                    return true;
                }
                else 
                {
                    return altDir.IsParallel(axis.Direction, angTol);
                }
            }

            return false;
        }

        /// <summary>
        /// Projects the specified point on this axis (find closest point)
        /// </summary>
        /// <param name="axis">This axis</param>
        /// <param name="pt">Point to project to</param>
        /// <returns>Projected point</returns>
        public static Point Project(this Axis axis, Point pt) 
        {
            var dir = axis.Direction.Normalize();
            var vec = axis.Point - pt;
            var dist = vec.Dot(dir);

            return axis.Point.Move(dir, dist);
        }

        /// <summary>
        /// Finds if 2 axes intersects
        /// </summary>
        /// <param name="thisAxis">This axis</param>
        /// <param name="otherAxis">Axis to check intersection with</param>
        /// <param name="point">Intersection point</param>
        /// <param name="tol">Tolerance</param>
        /// <returns>True if axes intersect</returns>
        public static bool Intersects(this Axis thisAxis, Axis otherAxis, out Point point,
            double tol = Numeric.DEFAULT_LENGTH_TOLERANCE)
        {
            var norm = thisAxis.Direction.Cross(otherAxis.Direction);

            var transformToXY = TransformMatrix.Compose(thisAxis.Direction, norm.Cross(thisAxis.Direction), norm, thisAxis.Point).Inverse();

            var firstDir = thisAxis.Direction.Transform(transformToXY);
            var firstPt = thisAxis.Point.Transform(transformToXY);

            var secondDir = otherAxis.Direction.Transform(transformToXY);
            var secondPt = otherAxis.Point.Transform(transformToXY);

            if (Numeric.Compare(firstPt.Z, 0, tol)
                && Numeric.Compare(firstDir.Z, 0, tol)
                && Numeric.Compare(secondDir.Z, 0, tol)
                && Numeric.Compare(secondPt.Z, 0, tol))
            {
                var denom = firstDir.Y * secondDir.X - firstDir.X * secondDir.Y;

                if (Math.Abs(denom) > tol)
                {
                    var t = ((firstPt.X - secondPt.X) * secondDir.Y + (secondPt.Y - firstPt.Y) * secondDir.X) / denom;

                    point = new Point(firstPt.X + firstDir.X * t, firstPt.Y + firstDir.Y * t, 0).Transform(transformToXY.Inverse());
                    return true;
                }
                else
                {
                    point = null;
                    return false;
                }
            }
            else
            {
                point = null;
                return false;
            }
        }
    }
}
