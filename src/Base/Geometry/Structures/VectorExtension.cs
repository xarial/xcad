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
        /// Gets the full angle (360 degress) between 2 vectors on the plane
        /// </summary>
        /// <param name="thisVec">First vector</param>
        /// <param name="otherVec">Second vector</param>
        /// <param name="plane">Plane to get angle at</param>
        /// <param name="tol">Vector tolerance</param>
        /// <returns>Angle in radians</returns>
        /// <exception cref="Exception">Vector must not be perpendicular to the plane</exception>
        /// <remarks>Vectors will be projected onto the plane</remarks>
        public static double GetAngleOnPlane(this Vector thisVec, Vector otherVec, Plane plane, double tol = Numeric.DEFAULT_NUMERIC_TOLERANCE) 
        {
            var thisProjVec = thisVec.Project(plane);

            if (thisProjVec.GetLength() < tol) 
            {
                throw new Exception("Vector is perpendicular to plane and cannot be projected");
            }

            var otherProjVec = otherVec.Project(plane);

            if (otherProjVec.GetLength() < tol)
            {
                throw new Exception("Other vector is perpendicular to plane and cannot be projected");
            }

            var invTransform = plane.GetTransformation().Inverse();

            thisProjVec = thisProjVec.Transform(invTransform);
            otherProjVec = otherProjVec.Transform(invTransform);

            var dot = thisProjVec.X * otherProjVec.X + thisProjVec.Y * otherProjVec.Y;
            var det = thisProjVec.X * otherProjVec.Y - thisProjVec.Y * otherProjVec.X;

            return Math.Atan2(det, dot);
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

        /// <summary>
        /// Projects vector onto the specified plane
        /// </summary>
        /// <param name="vec">Vector to project</param>
        /// <param name="plane">Target plane</param>
        /// <returns>Projected vector</returns>
        public static Vector Project(this Vector vec, Plane plane)
        {
            var invDenom = 1d / plane.Normal.Dot(plane.Normal);
            var d = plane.Normal.Dot(vec);

            return new Vector(
                vec.X - d * plane.Normal.X * invDenom,
                vec.Y - d * plane.Normal.Y * invDenom,
                vec.Z - d * plane.Normal.Z * invDenom);
        }
    }
}
