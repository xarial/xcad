//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.SolidWorks.Utils
{
    /// <summary>
    /// Utility to transform <see cref="TransformMatrix"/> and <see cref="IMathTransform"/>
    /// </summary>
    public static class TransformConverter
    {
        /// <summary>
        /// Transforms SOLIDWORKS matrix to xCAD matrix
        /// </summary>
        /// <param name="transform">Matrix to transform</param>
        /// <returns>Transformed matrix</returns>
        public static TransformMatrix ToTransformMatrix(this IMathTransform transform)
            => ToTransformMatrix(transform.ArrayData as double[]);

        /// <summary>
        /// Transforms data from SOLIDWORKS matrix <see cref="IMathTransform.ArrayData"/> to xCAD matrix
        /// </summary>
        /// <param name="data">Data tro transform</param>
        /// <returns>Transformed matrix</returns>
        public static TransformMatrix ToTransformMatrix(double[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length != 16)
            {
                throw new Exception("Array size must be 16 (4x4 matrix)");
            }

            var scale = data[12];

            return new TransformMatrix(
                data[0] * scale, data[1] * scale, data[2] * scale, 0,
                data[3] * scale, data[4] * scale, data[5] * scale, 0,
                data[6] * scale, data[7] * scale, data[8] * scale, 0,
                data[9], data[10], data[11], 1);
        }


        /// <summary>
        /// Transforms xCAD matrix to SOLIDWORKS transform
        /// </summary>
        /// <param name="mathUtils">SOLIDWORKS math utility</param>
        /// <param name="matrix">Matrix to transform</param>
        /// <returns>SOLIDWORKS transform</returns>
        public static IMathTransform ToMathTransform(this IMathUtility mathUtils, TransformMatrix matrix)
            => mathUtils.CreateTransform(ToMathTransformData(matrix)) as IMathTransform;

        /// <summary>
        /// Transforms xCAD matrix to SOLIDWORKS transform data <see cref="IMathTransform.ArrayData"/>
        /// </summary>
        /// <param name="matrix">Matrix to transform</param>
        /// <returns>SOLIDWORKS transform data</returns>
        public static double[] ToMathTransformData(this TransformMatrix matrix)
        {
            var transX = matrix.M41;
            var transY = matrix.M42;
            var transZ = matrix.M43;

            var scaleX = new Vector(matrix.M11, matrix.M21, matrix.M31).GetLength();
            var scaleY = new Vector(matrix.M11, matrix.M21, matrix.M31).GetLength();
            var scaleZ = new Vector(matrix.M11, matrix.M21, matrix.M31).GetLength();

            return new double[]
            {
                matrix.M11 / scaleX, matrix.M12 / scaleY, matrix.M13 / scaleZ,
                matrix.M21 / scaleX, matrix.M22 / scaleY, matrix.M23 / scaleZ,
                matrix.M31 / scaleX, matrix.M32 / scaleY, matrix.M33 / scaleZ,
                transX, transY, transZ,
                scaleX,
                0, 0, 0
            };
        }
    }
}