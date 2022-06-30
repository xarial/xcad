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
    }
}
