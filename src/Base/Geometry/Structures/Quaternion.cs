//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Diagnostics;
using System.Numerics;

namespace Xarial.XCad.Geometry.Structures
{
    /// <summary>
    /// Represents rotation in 3D space
    /// </summary>
    [DebuggerDisplay("{" + nameof(X) + "}, {" + nameof(Y) + "}, {" + nameof(Z) + "}, {" + nameof(W) + "}")]
    public class Quaternion
    {
        /// <summary>
        /// Creates from transformation matrix
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <returns>Quaternion</returns>
        public static Quaternion FromMatrix(TransformMatrix matrix)
        {
            var trace = matrix.M11 + matrix.M22 + matrix.M33;
            double x, y, z, w;

            if (trace > 0)
            {
                var s = Math.Sqrt(trace + 1.0);
                var sInv = 0.5 / s;
                w = 0.5 * s;
                x = (matrix.M23 - matrix.M32) * sInv;
                y = (matrix.M31 - matrix.M13) * sInv;
                z = (matrix.M12 - matrix.M21) * sInv;
            }
            else if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {
                var s = Math.Sqrt(1.0 + matrix.M11 - matrix.M22 - matrix.M33);
                var sInv = 0.5 / s;

                w = (matrix.M23 - matrix.M32) * sInv;
                x = 0.5 * s;
                y = (matrix.M12 + matrix.M21) * sInv;
                z = (matrix.M13 + matrix.M31) * sInv;
            }
            else if (matrix.M22 > matrix.M33)
            {
                var s = Math.Sqrt(1.0 + matrix.M22 - matrix.M11 - matrix.M33);
                var sInv = 0.5 / s;
                w = (matrix.M31 - matrix.M13) * sInv;
                x = (matrix.M12 + matrix.M21) * sInv;
                y = 0.5 * s;
                z = (matrix.M23 + matrix.M32) * sInv;
            }
            else
            {
                var s = Math.Sqrt(1.0 + matrix.M33 - matrix.M11 - matrix.M22);
                var sInv = 0.5 / s;
                w = (matrix.M12 - matrix.M21) * sInv;
                x = (matrix.M13 + matrix.M31) * sInv;
                y = (matrix.M23 + matrix.M32) * sInv;
                z = 0.5 * s;
            }

            return new Quaternion(x, y, z, w);
        }

        /// <summary>
        /// X-component of vector part
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Y-component of vector part
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// Z-component of vector part
        /// </summary>
        public double Z { get; }

        /// <summary>
        /// Scalar part (real component)
        /// </summary>
        public double W { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Quaternion(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// Length of the quaternion
        /// </summary>
        public double Length => Math.Sqrt(X * X + Y * Y + Z * Z + W * W);

        /// <summary>
        /// Returns normalized quaternion
        /// </summary>
        /// <returns></returns>
        public Quaternion Normalize()
        {
            var length = Length;
            return new Quaternion(X / length, Y / length, Z / length, W / length);
        }
    }

    /// <summary>
    /// Aditional methods of <see cref="Quaternion"/>
    /// </summary>
    public static class QuaternionExtension 
    {
        /// <summary>
        /// Gets euler angles
        /// </summary>
        /// <param name="quaternion">Input quternion</param>
        /// <param name="yaw">Counterclockwise rotation about z-axis</param>
        /// <param name="pitch">Counterclockwise rotation about y-axis</param>
        /// <param name="roll">Counterclockwise rotation about x-axis</param>
        /// <param name="singTol">Tolerance for singularity</param>
        public static void GetEulerAngles(this Quaternion quaternion, out double yaw, out double pitch, out double roll, double singTol = 0.0499)
        {
            var sqw = quaternion.W * quaternion.W;
            var sqx = quaternion.X * quaternion.X;
            var sqy = quaternion.Y * quaternion.Y;
            var sqz = quaternion.Z * quaternion.Z;
            var unit = sqx + sqy + sqz + sqw;
            var test = quaternion.X * quaternion.Y + quaternion.Z * quaternion.W;

            if (test > singTol * unit)//singularity at south pole
            {
                yaw = 2 * Math.Atan2(quaternion.X, quaternion.W);
                pitch = Math.PI / 2;
                roll = 0;
                return;
            }
            if (test < -singTol * unit)//singularity at south pole
            {
                yaw = -2 * Math.Atan2(quaternion.X, quaternion.W);
                pitch = -Math.PI / 2;
                roll = 0;
                return;
            }
            else
            {
                yaw = Math.Atan2(2 * quaternion.Y * quaternion.W - 2 * quaternion.X * quaternion.Z, sqx - sqy - sqz + sqw);
                pitch = Math.Asin(2 * test / unit);
                roll = Math.Atan2(2 * quaternion.X * quaternion.W - 2 * quaternion.Y * quaternion.Z, -sqx + sqy - sqz + sqw);
            }
        }
    }
}