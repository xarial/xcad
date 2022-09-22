//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
    /// Additional methods for the vector
    /// </summary>
    public static class VectorExtension
    {
        /// <summary>
        /// Creates perpendicular vector
        /// </summary>
        /// <param name="dir">Vector to base on</param>
        /// <param name="tol">Tolerance</param>
        /// <returns></returns>
        public static Vector CreateAnyPerpendicular(this Vector dir, double tol = Numeric.DEFAULT_ANGLE_TOLERANCE)
        {
            Vector refDir;
            var zVec = new Vector(0, 0, 1);

            if (dir.IsParallel(zVec, tol))
            {
                refDir = new Vector(1, 0, 0);
            }
            else
            {
                refDir = dir.Cross(zVec);
            }

            return refDir;
        }

        /// <summary>
        /// Finds the angle between vectors
        /// </summary>
        /// <param name="thisVec">First vector</param>
        /// <param name="otherVec">Other vector</param>
        /// <param name="tol">Tolerance</param>
        /// <returns>Angle in radians</returns>
        public static double GetAngle(this Vector thisVec, Vector otherVec, double tol = Numeric.DEFAULT_NUMERIC_TOLERANCE)
        {
            var cosine = thisVec.Dot(otherVec) / (thisVec.GetLength() * otherVec.GetLength());

            if (cosine > 1)
            {
                if (Math.Abs(cosine - 1) < tol)
                {
                    cosine = 1;
                }
                else
                {
                    throw new Exception("Invalid value");
                }
            }
            else if (cosine < -1)
            {
                if (Math.Abs(cosine + 1) < tol)
                {
                    cosine = -1;
                }
                else
                {
                    throw new Exception("Invalid value");
                }
            }

            return Math.Acos(cosine);
        }

        /// <summary>
        /// Checks if 2 vectors are parallel
        /// </summary>
        /// <param name="firstVec">First vector</param>
        /// <param name="secondVec">Second vector</param>
        /// <param name="tol">Angle tolerance</param>
        /// <returns>True if vectors are parallel, False if not</returns>
        public static bool IsParallel(this Vector firstVec, Vector secondVec, double tol = Numeric.DEFAULT_ANGLE_TOLERANCE)
        {
            var ang = firstVec.GetAngle(secondVec);

            return Math.Abs(ang) < tol || Math.PI - Math.Abs(ang) < tol;
        }

        /// <summary>
        /// Finds the angle between this vector and plane
        /// </summary>
        /// <param name="vec">Vector</param>
        /// <param name="plane">Plane</param>
        /// <returns>Angle</returns>
        public static double GetAngle(this Vector vec, Plane plane)
            => Math.PI / 2 - vec.GetAngle(plane.Normal);
    }
}
