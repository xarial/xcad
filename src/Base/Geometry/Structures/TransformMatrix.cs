//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Drawing.Drawing2D;
using System.Numerics;

namespace Xarial.XCad.Geometry.Structures
{
    /// <summary>
    /// Represents 4x4 transformation matrix
    /// </summary>
    public class TransformMatrix
    {
        private static double GetMaterixElementAtIndex(double[] matrix, int index)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException(nameof(matrix));
            }

            if (matrix.Length != 16)
            {
                throw new ArgumentException("Invalid matrix format. Array must have 16 elements for 4x4 matrix");
            }

            return matrix[index];
        }

        private static readonly double[] m_Identity
            = new double[]
            {
                1.0, 0.0, 0.0, 0.0,
                0.0, 1.0, 0.0, 0.0,
                0.0, 0.0, 1.0, 0.0,
                0.0, 0.0, 0.0, 1.0
            };

        /// <summary>
        /// Returns identity matrix
        /// </summary>
        public static TransformMatrix Identity
            => new TransformMatrix(m_Identity);

        /// <summary>
        /// Create rotation transformation around axis
        /// </summary>
        /// <param name="axis">Axis to generate rotation about</param>
        /// <param name="angle">Angle in radians</param>
        /// <param name="point">Anchor point of the rotation</param>
        /// <returns>Transformation matrix</returns>
        public static TransformMatrix CreateFromRotationAroundAxis(Vector axis, double angle, Point point)
        {
            var s = Math.Sin(angle);
            var c = Math.Cos(angle);

            var normVec = axis.Normalize();

            var x = normVec.X;
            var y = normVec.Y;
            var z = normVec.Z;

            var xx = x * x;
            var yy = y * y;
            var zz = z * z;
            var xy = x * y;
            var xz = x * z;
            var yz = y * z;

            var matrix = new TransformMatrix(
                xx + c * (1.0 - xx), xy - c * xy + s * z, xz - c * xz - s * y, 0.0,
                xy - c * xy - s * z, yy + c * (1.0 - yy), yz - c * yz + s * x, 0.0,
                xz - c * xz + s * y, yz - c * yz - s * x, zz + c * (1.0 - zz), 0.0,
                0.0, 0.0, 0.0, 1.0);

            if (Math.Abs(point.X) > double.Epsilon
                || Math.Abs(point.Y) > double.Epsilon
                || Math.Abs(point.Z) > double.Epsilon)
            {
                var translateMatrix = CreateFromTranslation(point.ToVector());

                var translateMatrixInv = CreateFromTranslation(point.ToVector() * -1);

                matrix = translateMatrixInv.Multiply(matrix).Multiply(translateMatrix);
            }

            return matrix;
        }

        /// <summary>
        /// Creates matrix from quaternion and translation
        /// </summary>
        /// <param name="quaternion">Transformation component</param>
        /// <param name="translation">Rotation component</param>
        /// <returns>Matrix</returns>
        public static TransformMatrix CreateFromQuaternionAndTranslation(Quaternion quaternion, Vector translation)
        {
            quaternion = quaternion.Normalize();

            var xx = quaternion.X * quaternion.X;
            var yy = quaternion.Y * quaternion.Y;
            var zz = quaternion.Z * quaternion.Z;
            var xy = quaternion.X * quaternion.Y;
            var xz = quaternion.X * quaternion.Z;
            var yz = quaternion.Y * quaternion.Z;
            var wx = quaternion.W * quaternion.X;
            var wy = quaternion.W * quaternion.Y;
            var wz = quaternion.W * quaternion.Z;

            return new TransformMatrix(
                1 - 2 * (yy + zz), 2 * (xy + wz), 2 * (xz - wy), 0,
                2 * (xy - wz), 1 - 2 * (xx + zz), 2 * (yz + wx), 0,
                2 * (xz + wy), 2 * (yz - wx), 1 - 2 * (xx + yy), 0,
                translation.X, translation.Y, translation.Z, 1);
        }

        /// <summary>
        /// Creates matrix from translation
        /// </summary>
        ///<param name="x">Translation in X direction</param>
        ///<param name="y">Translation in Y direction</param>
        ///<param name="z">Translation in Z direction</param>
        /// <returns>Transformation matrix</returns>
        public static TransformMatrix CreateFromTranslation(double x, double y, double z)
            => new TransformMatrix(1.0, 0.0, 0.0, 0.0,
                0.0, 1.0, 0.0, 0.0,
                0.0, 0.0, 1.0, 0.0,
                x, y, z, 1.0);


        /// <summary>
        /// Creates matrix from translation
        /// </summary>
        /// <param name="translation">Translation component</param>
        /// <returns>Transformation matrix</returns>
        public static TransformMatrix CreateFromTranslation(Vector translation)
            => CreateFromTranslation(translation.X, translation.Y, translation.Z);

        /// <summary>
        /// Creates matrix from scale vector
        /// </summary>
        /// <param name="scale">X, Y, Z directions scale</param>
        /// <returns>Transformation matrix</returns>
        public static TransformMatrix CreateFromScale(Vector scale)
            => new TransformMatrix(
                scale.X, 0, 0, 0,
                0, scale.Y, 0, 0,
                0, 0, scale.Z, 0,
                0, 0, 0, 1);

        /// <summary>
        /// Creates transformation from the reflection
        /// </summary>
        /// <param name="plane">Reflection plane</param>
        /// <returns>Transformation matrix</returns>
        public static TransformMatrix CreateFromReflection(Plane plane)
        {
            var planeNormal = plane.Normal.Normalize();

            var planeDist = plane.GetDistance(new Point(0, 0, 0));

            var a = planeNormal.X;
            var b = planeNormal.Y;
            var c = planeNormal.Z;

            var fa = -2.0d * a;
            var fb = -2.0d * b;
            var fc = -2.0d * c;

            return new TransformMatrix(
                fa * a + 1.0,
                fb * a,
                fc * a,
                0.0,

                fa * b,
                fb * b + 1.0,
                fc * b,
                0.0,

                fa * c,
                fb * c,
                fc * c + 1.0,
                0.0,

                fa * planeDist,
                fb * planeDist,
                fc * planeDist,
                1.0f);
        }

        /// <summary>
        /// Composes the transformation matrix from input parameters
        /// </summary>
        /// <param name="axisX">X axis</param>
        /// <param name="axisY">Y axis</param>
        /// <param name="axisZ">Z axis</param>
        /// <param name="position">Position coordinate</param>
        /// <returns>Transformation matrix</returns>
        public static TransformMatrix Compose(Vector axisX, Vector axisY, Vector axisZ, Point position)
        {
            axisX = axisX.Normalize();
            axisY = axisY.Normalize();
            axisZ = axisZ.Normalize();

            return new TransformMatrix(
                axisX.X, axisX.Y, axisX.Z, 0.0,
                axisY.X, axisY.Y, axisY.Z, 0.0,
                axisZ.X, axisZ.Y, axisZ.Z, 0.0,
                position.X, position.Y, position.Z, 1.0);
        }

        /// <summary>
        /// Composes the transformation matrix from rotation and translation
        /// </summary>
        /// <param name="yaw">Counterclockwise rotation about z-axis</param>
        /// <param name="pitch">Counterclockwise rotation about y-axis</param>
        /// <param name="roll">Counterclockwise rotation about x-axis</param>
        ///<param name="x">Translation in X direction</param>
        ///<param name="y">Translation in Y direction</param>
        ///<param name="z">Translation in Z direction</param>
        /// <returns>Transformation matrix</returns>
        public static TransformMatrix Compose(double yaw, double pitch, double roll, double x, double y, double z)
            => new TransformMatrix(
                Math.Cos(yaw) * Math.Cos(pitch), Math.Cos(yaw) * Math.Sin(pitch) * Math.Sin(roll) - Math.Sin(yaw) * Math.Cos(roll), Math.Cos(yaw) * Math.Sin(pitch) * Math.Cos(roll) + Math.Sin(yaw) * Math.Sin(roll), 0.0,
                Math.Sin(yaw) * Math.Cos(pitch), Math.Sin(yaw) * Math.Sin(pitch) * Math.Sin(roll) + Math.Cos(yaw) * Math.Cos(roll), Math.Sin(yaw) * Math.Sin(pitch) * Math.Cos(roll) - Math.Cos(yaw) * Math.Sin(roll), 0.0,
                -Math.Sin(pitch), Math.Cos(pitch) * Math.Sin(roll), Math.Cos(pitch) * Math.Cos(roll), 0.0,
                x, y, z, 1.0);

        /// <summary>
        /// Creates rotation matrix from rotation angles
        /// </summary>
        /// <param name="yaw">Counterclockwise rotation about z-axis</param>
        /// <param name="pitch">Counterclockwise rotation about y-axis</param>
        /// <param name="roll">Counterclockwise rotation about x-axis</param>
        /// <returns>Transformation matrix</returns>
        public static TransformMatrix CreateFromRotation(double yaw, double pitch, double roll)
            => Compose(yaw, pitch, roll, 0, 0, 0);

        /// <summary>
        /// X-Axis Rotation (X)
        /// </summary>
        public double M11 { get; }

        /// <summary>
        /// X-Axis Rotation (Y)
        /// </summary>
        public double M12 { get; }

        /// <summary>
        /// X-Axis Rotation (Z)
        /// </summary>
        public double M13 { get; }

        /// <summary>
        /// 0 - Not Used
        /// </summary>
        public double M14 { get; }

        /// <summary>
        /// Y-Axis Rotation (X)
        /// </summary>
        public double M21 { get; }

        /// <summary>
        /// Y-Axis Rotation (Y)
        /// </summary>
        public double M22 { get; }

        /// <summary>
        /// Y-Axis Rotation (Z)
        /// </summary>
        public double M23 { get; }

        /// <summary>
        /// 0 - Not Used
        /// </summary>
        public double M24 { get; }

        /// <summary>
        /// Z-Axis Rotation (X)
        /// </summary>
        public double M31 { get; }

        /// <summary>
        /// Z-Axis Rotation (Y)
        /// </summary>
        public double M32 { get; }

        /// <summary>
        /// Z-Axis Rotation (Z)
        /// </summary>
        public double M33 { get; }

        /// <summary>
        /// 0 - Not Used
        /// </summary>
        public double M34 { get; }

        /// <summary>
        /// X-Translation
        /// </summary>
        public double M41 { get; }

        /// <summary>
        /// Y-Translation
        /// </summary>
        public double M42 { get; }

        /// <summary>
        /// Z-Translation
        /// </summary>
        public double M43 { get; }

        /// <summary>
        /// 1
        /// </summary>
        public double M44 { get; }

        /// <summary>
        /// Creates identity transformation matrix
        /// </summary>
        public TransformMatrix()
            : this(m_Identity)
        {
        }

        /// <summary>
        /// Creates transformation matrix from input data
        /// </summary>
        public TransformMatrix(double m11, double m12, double m13, double m14,
            double m21, double m22, double m23, double m24,
            double m31, double m32, double m33, double m34,
            double m41, double m42, double m43, double m44)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M14 = m14;

            M21 = m21;
            M22 = m22;
            M23 = m23;
            M24 = m24;

            M31 = m31;
            M32 = m32;
            M33 = m33;
            M34 = m34;

            M41 = m41;
            M42 = m42;
            M43 = m43;
            M44 = m44;
        }

        /// <summary>
        /// Creates transform matrix from an array
        /// </summary>
        /// <param name="matrix">Array of 16 elements</param>
        public TransformMatrix(double[] matrix) :
            this(GetMaterixElementAtIndex(matrix, 0), GetMaterixElementAtIndex(matrix, 1), GetMaterixElementAtIndex(matrix, 2), GetMaterixElementAtIndex(matrix, 3),
                GetMaterixElementAtIndex(matrix, 4), GetMaterixElementAtIndex(matrix, 5), GetMaterixElementAtIndex(matrix, 6), GetMaterixElementAtIndex(matrix, 7),
                GetMaterixElementAtIndex(matrix, 8), GetMaterixElementAtIndex(matrix, 9), GetMaterixElementAtIndex(matrix, 10), GetMaterixElementAtIndex(matrix, 11),
                GetMaterixElementAtIndex(matrix, 12), GetMaterixElementAtIndex(matrix, 13), GetMaterixElementAtIndex(matrix, 14), GetMaterixElementAtIndex(matrix, 15))
        {
        }

        /// <summary>
        /// Returns the determinant of the matrix
        /// </summary>
        public double Determinant
            => M11 * M22 * M33 * M44 - M11 * M22 * M34 * M43 + M11 * M23 * M34 * M42 - M11 * M23 * M32 * M44
               + M11 * M24 * M32 * M43 - M11 * M24 * M33 * M42 - M12 * M23 * M34 * M41 + M12 * M23 * M31 * M44
               - M12 * M24 * M31 * M43 + M12 * M24 * M33 * M41 - M12 * M21 * M33 * M44 + M12 * M21 * M34 * M43
               + M13 * M24 * M31 * M42 - M13 * M24 * M32 * M41 + M13 * M21 * M32 * M44 - M13 * M21 * M34 * M42
               + M13 * M22 * M34 * M41 - M13 * M22 * M31 * M44 - M14 * M21 * M32 * M43 + M14 * M21 * M33 * M42
               - M14 * M22 * M33 * M41 + M14 * M22 * M31 * M43 - M14 * M23 * M31 * M42 + M14 * M23 * M32 * M41;

        /// <summary>
        /// Translation component of the matrix
        /// </summary>
        public Vector Translation => new Vector(M41, M42, M43);

        /// <summary>
        /// Scale in X, Y, Z directions
        /// </summary>
        public Vector Scale => new Vector(
            new Vector(M11, M21, M31).GetLength(),
            new Vector(M12, M22, M32).GetLength(),
            new Vector(M13, M23, M33).GetLength());

        /// <summary>
        /// Multiplies transformation matrix
        /// </summary>
        /// <param name="matrix">Matrix to multiply with</param>
        /// <returns>Resulting matrix</returns>
        public TransformMatrix Multiply(TransformMatrix matrix)
            => new TransformMatrix(
                (M11 * matrix.M11) + (M12 * matrix.M21) + (M13 * matrix.M31) + (M14 * matrix.M41),
                (M11 * matrix.M12) + (M12 * matrix.M22) + (M13 * matrix.M32) + (M14 * matrix.M42),
                (M11 * matrix.M13) + (M12 * matrix.M23) + (M13 * matrix.M33) + (M14 * matrix.M43),
                (M11 * matrix.M14) + (M12 * matrix.M24) + (M13 * matrix.M34) + (M14 * matrix.M44),
                (M21 * matrix.M11) + (M22 * matrix.M21) + (M23 * matrix.M31) + (M24 * matrix.M41),
                (M21 * matrix.M12) + (M22 * matrix.M22) + (M23 * matrix.M32) + (M24 * matrix.M42),
                (M21 * matrix.M13) + (M22 * matrix.M23) + (M23 * matrix.M33) + (M24 * matrix.M43),
                (M21 * matrix.M14) + (M22 * matrix.M24) + (M23 * matrix.M34) + (M24 * matrix.M44),
                (M31 * matrix.M11) + (M32 * matrix.M21) + (M33 * matrix.M31) + (M34 * matrix.M41),
                (M31 * matrix.M12) + (M32 * matrix.M22) + (M33 * matrix.M32) + (M34 * matrix.M42),
                (M31 * matrix.M13) + (M32 * matrix.M23) + (M33 * matrix.M33) + (M34 * matrix.M43),
                (M31 * matrix.M14) + (M32 * matrix.M24) + (M33 * matrix.M34) + (M34 * matrix.M44),
                (M41 * matrix.M11) + (M42 * matrix.M21) + (M43 * matrix.M31) + (M44 * matrix.M41),
                (M41 * matrix.M12) + (M42 * matrix.M22) + (M43 * matrix.M32) + (M44 * matrix.M42),
                (M41 * matrix.M13) + (M42 * matrix.M23) + (M43 * matrix.M33) + (M44 * matrix.M43),
                (M41 * matrix.M14) + (M42 * matrix.M24) + (M43 * matrix.M34) + (M44 * matrix.M44));

        /// <summary>
        /// Adds matrix
        /// </summary>
        /// <param name="matrix">Matrix to add</param>
        /// <returns>Added matrix</returns>
        public TransformMatrix Add(TransformMatrix matrix)
            => new TransformMatrix(
                M11 + matrix.M11, M12 + matrix.M12, M13 + matrix.M13, M14 + matrix.M14,
                M21 + matrix.M21, M22 + matrix.M22, M23 + matrix.M23, M24 + matrix.M24,
                M31 + matrix.M31, M32 + matrix.M32, M33 + matrix.M33, M34 + matrix.M34,
                M41 + matrix.M41, M42 + matrix.M42, M43 + matrix.M43, M44 + matrix.M44);

        /// <summary>
        /// Subtracts matrix
        /// </summary>
        /// <param name="matrix">Matrix to subtract</param>
        /// <returns>Subtracted matrix</returns>
        public TransformMatrix Subtract(TransformMatrix matrix)
            => new TransformMatrix(
                M11 - matrix.M11, M12 - matrix.M12, M13 - matrix.M13, M14 - matrix.M14,
                M21 - matrix.M21, M22 - matrix.M22, M23 - matrix.M23, M24 - matrix.M24,
                M31 - matrix.M31, M32 - matrix.M32, M33 - matrix.M33, M34 - matrix.M34,
                M41 - matrix.M41, M42 - matrix.M42, M43 - matrix.M43, M44 - matrix.M44);

        /// <summary>
        /// Inverses this matrix
        /// </summary>
        /// <returns>Inversed matrix</returns>
        public TransformMatrix Inverse()
        {
            var det = Determinant;

            if (Math.Abs(det) < double.Epsilon)
            {
                throw new Exception("Singular matrix cannot be inverted");
            }

            var detInv = 1.0 / det;

            var m33443443 = M33 * M44 - M34 * M43;
            var m32443442 = M32 * M44 - M34 * M42;
            var m32433342 = M32 * M43 - M33 * M42;
            var m31443441 = M31 * M44 - M34 * M41;
            var m31433341 = M31 * M43 - M33 * M41;
            var m31423241 = M31 * M42 - M32 * M41;
            var m23442443 = M23 * M44 - M24 * M43;
            var m22442442 = M22 * M44 - M24 * M42;
            var m22432342 = M22 * M43 - M23 * M42;
            var m21442441 = M21 * M44 - M24 * M41;
            var m21432341 = M21 * M43 - M23 * M41;
            var m21422241 = M21 * M42 - M22 * M41;
            var m23342433 = M23 * M34 - M24 * M33;
            var m22342432 = M22 * M34 - M24 * M32;
            var m22332332 = M22 * M33 - M23 * M32;
            var m21342431 = M21 * M34 - M24 * M31;
            var m21332331 = M21 * M33 - M23 * M31;
            var m21322231 = M21 * M32 - M22 * M31;

            var a1 = +(M22 * m33443443 - M23 * m32443442 + M24 * m32433342);
            var a2 = -(M21 * m33443443 - M23 * m31443441 + M24 * m31433341);
            var a3 = +(M21 * m32443442 - M22 * m31443441 + M24 * m31423241);
            var a4 = -(M21 * m32433342 - M22 * m31433341 + M23 * m31423241);

            return new TransformMatrix(
                a1 * detInv,
                -(M12 * m33443443 - M13 * m32443442 + M14 * m32433342) * detInv,
                (M12 * m23442443 - M13 * m22442442 + M14 * m22432342) * detInv,
                -(M12 * m23342433 - M13 * m22342432 + M14 * m22332332) * detInv,

                a2 * detInv,
                (M11 * m33443443 - M13 * m31443441 + M14 * m31433341) * detInv,
                -(M11 * m23442443 - M13 * m21442441 + M14 * m21432341) * detInv,
                (M11 * m23342433 - M13 * m21342431 + M14 * m21332331) * detInv,

                a3 * detInv,
                -(M11 * m32443442 - M12 * m31443441 + M14 * m31423241) * detInv,
                (M11 * m22442442 - M12 * m21442441 + M14 * m21422241) * detInv,
                -(M11 * m22342432 - M12 * m21342431 + M14 * m21322231) * detInv,

                a4 * detInv,
                (M11 * m32433342 - M12 * m31433341 + M13 * m31423241) * detInv,
                -(M11 * m22432342 - M12 * m21432341 + M13 * m21422241) * detInv,
                (M11 * m22332332 - M12 * m21332331 + M13 * m21322231) * detInv);
        }

        /// <summary>
        /// Converts matrix to 1-dimensional array
        /// </summary>
        /// <returns></returns>
        public double[] ToArray()
            => new double[]
            {
                M11, M12, M13, M14,
                M21, M22, M23, M24,
                M31, M32, M33, M34,
                M41, M42, M43, M44
            };

        /// <summary>
        /// Multiplies two matrices
        /// </summary>
        /// <param name="srcMatrix">Source matrix</param>
        /// <param name="other">Other matrix</param>
        /// <returns>Transformed matrix</returns>
        public static TransformMatrix operator *(TransformMatrix srcMatrix, TransformMatrix other)
            => srcMatrix.Multiply(other);

        /// <summary>
        /// Adds two matrices
        /// </summary>
        /// <param name="srcMatrix">Source matrix</param>
        /// <param name="other">Other matrix</param>
        /// <returns>Transformed matrix</returns>
        public static TransformMatrix operator +(TransformMatrix srcMatrix, TransformMatrix other)
            => srcMatrix.Add(other);

        /// <summary>
        /// Subtracts two matrices
        /// </summary>
        /// <param name="srcMatrix">Source matrix</param>
        /// <param name="other">Other matrix</param>
        /// <returns>Transformed matrix</returns>
        public static TransformMatrix operator -(TransformMatrix srcMatrix, TransformMatrix other)
            => srcMatrix.Subtract(other);

        /// <summary>
        /// Converts to string
        /// </summary>
        public override string ToString() => string.Join(", ", ToArray());
    }

    /// <summary>
    /// Additional methods of <see cref="TransformMatrix"/>
    /// </summary>
    public static class TransformMatrixExtension 
    {
        ///<summary>Returns angles of this matrix</summary>
        /// <param name="matrix">Input matrix</param>
        /// <param name="yaw">Counterclockwise rotation about z-axis</param>
        /// <param name="pitch">Counterclockwise rotation about y-axis</param>
        /// <param name="roll">Counterclockwise rotation about x-axis</param>
        /// <param name="gimbalLockTol">Tolerance for Gimbal lock check</param>
        public static void GetEulerAngles(this TransformMatrix matrix, out double yaw, out double pitch, out double roll, double gimbalLockTol = 1e-6)
        {
            var xAxis = new Vector(matrix.M11, matrix.M12, matrix.M13).Normalize();
            var yAxis = new Vector(matrix.M21, matrix.M22, matrix.M23).Normalize();
            var zAxis = new Vector(matrix.M31, matrix.M32, matrix.M33).Normalize();

            pitch = Math.Asin(-zAxis.X);

            //Gimbal lock check
            if (Math.Cos(pitch) > gimbalLockTol)
            {
                yaw = Math.Atan2(yAxis.X, xAxis.X);
                roll = Math.Atan2(zAxis.Y, zAxis.Z);
            }
            else
            {
                yaw = Math.Atan2(-xAxis.Y, yAxis.Y);
                roll = 0;
            }
        }
    }
}