//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
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
        /// <param name="tol">Tolerance</param>
        /// <returns>True if collinear, false if not</returns>
        public static bool IsCollinear(this Axis axis, Axis otherAxis, double tol = Numeric.DEFAULT_ANGLE_TOLERANCE) 
        {
            if (axis.Direction.IsParallel(otherAxis.Direction, tol)) 
            {
                var altDir = axis.Point - otherAxis.Point;

                if (altDir.GetLength() < Numeric.DEFAULT_LENGTH_TOLERANCE)
                {
                    return true;
                }
                else 
                {
                    return altDir.IsParallel(axis.Direction, tol);
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
    }
}
