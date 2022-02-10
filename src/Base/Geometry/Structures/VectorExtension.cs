//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

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
        /// <returns></returns>
        public static Vector CreateAnyPerpendicular(this Vector dir)
        {
            Vector refDir;
            var zVec = new Vector(0, 0, 1);

            if (dir.IsSame(zVec))
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
        /// <returns>Angle in radians</returns>
        public static double GetAngle(this Vector thisVec, Vector otherVec)
        {
            var TOL = 1E-12;

            var cosine = thisVec.Dot(otherVec) / (thisVec.GetLength() * otherVec.GetLength());

            if (cosine > 1)
            {
                if (Math.Abs(cosine - 1) < TOL)
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
                if (Math.Abs(cosine + 1) < TOL)
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
    }
}
