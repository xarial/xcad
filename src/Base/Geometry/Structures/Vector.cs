//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.Geometry.Structures
{
    /// <inheritdoc/>
    /// <summary>
    /// Represents the vector
    /// </summary>
    public class Vector : Point
    {
        public static Vector operator *(Vector vec, double scale)
        {
            var res = new Vector(vec);
            res.Scale(scale);
            return res;
        }

        /// <inheritdoc cref="Point(double, double, double)"/>
        /// <summary>
        /// Creates vector by 3 coordinates
        /// </summary>
        public Vector(double x, double y, double z) : base(x, y, z)
        {
        }

        /// <summary>
        /// Creates vector by direction
        /// </summary>
        /// <param name="dir">Direction of vector</param>
        public Vector(double[] dir) : base(dir)
        {
        }

        /// <summary>
        /// Creates vector by as a copy of another vector
        /// </summary>
        /// <param name="vec">Vector to copy</param>
        public Vector(Vector vec) : base(vec.X, vec.Y, vec.Z)
        {
        }

        /// <summary>
        /// Compares the vectors
        /// </summary>
        /// <param name="vec">Another vector</param>
        /// <param name="normilize">Normalized vectors while comparison</param>
        /// <returns>True if vectors are the same</returns>
        public bool IsSame(Vector vec, bool normilize = true)
        {
            if (vec == null)
            {
                throw new ArgumentNullException(nameof(vec));
            }

            if (normilize)
            {
                var thisNorm = this.Normalize();
                var otherNorm = vec.Normalize();

                return thisNorm.IsSame(otherNorm.X, otherNorm.Y, otherNorm.Z)
                    || thisNorm.IsSame(-otherNorm.X, -otherNorm.Y, -otherNorm.Z);
            }
            else
            {
                return IsSame(vec.X, vec.Y, vec.Z);
            }
        }

        /// <summary>
        /// Normalizes the vector
        /// </summary>
        /// <returns>New normalized vector</returns>
        public Vector Normalize()
        {
            var thisLen = GetLength();
            var thisNorm = new Vector(X / thisLen, Y / thisLen, Z / thisLen);
            return thisNorm;
        }

        /// <summary>
        /// Creates a cross product of this vector with another vector
        /// </summary>
        /// <param name="vector">Another vector</param>
        /// <returns>New cross product vector</returns>
        public Vector Cross(Vector vector)
        {
            var x = Y * vector.Z - vector.Y * Z; ;
            var y = (X * vector.Z - vector.X * Z) * -1;
            var z = X * vector.Y - vector.X * Y;

            return new Vector(x, y, z);
        }

        /// <summary>
        /// Returns the length of this vector
        /// </summary>
        /// <returns>Vector length</returns>
        public double GetLength()
        {
            return Math.Sqrt(Math.Pow(X, 2)
                + Math.Pow(Y, 2)
                + Math.Pow(Z, 2));
        }

        /// <summary>
        /// Calculates the dot product between vectors
        /// </summary>
        /// <param name="vector">Other vector</param>
        /// <returns>Scalar dot product</returns>
        public double Dot(Vector vector)
            => X * vector.X + Y * vector.Y + Z * vector.Z;

        /// <summary>
        /// Transforms this vector with the transformation matrix
        /// </summary>
        /// <param name="matrix">Transformation matrix</param>
        /// <returns>Transformed vector</returns>
        public new Vector Transform(TransformMatrix matrix)
            => new Vector(
                X * matrix.M11 + Y * matrix.M21 + Z * matrix.M31,
                X * matrix.M12 + Y * matrix.M22 + Z * matrix.M32,
                X * matrix.M13 + Y * matrix.M23 + Z * matrix.M33);

        /// <summary>
        /// Applies the transformation to the vector
        /// </summary>
        /// <param name="vec">Source vector</param>
        /// <param name="matrix">Matrix</param>
        /// <returns>Transformed point</returns>
        public static Vector operator *(Vector vec, TransformMatrix matrix)
            => vec.Transform(matrix);
    }
}