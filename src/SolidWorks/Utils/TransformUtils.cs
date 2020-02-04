//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal static class TransformUtils
    {
        internal static TransformMatrix ToTransformMatrix(IMathTransform transform)
        {
            return ToTransformMatrix(transform.ArrayData as double[]);
        }

        internal static TransformMatrix ToTransformMatrix(double[] data)
        {
            return new TransformMatrix(
                data[0] * data[12], data[1], data[2], data[13],
                data[3], data[4] * data[12], data[5], data[14],
                data[6], data[7], data[8] * data[12], data[15],
                data[9], data[10], data[11], 1);
        }

        internal static IMathTransform ToMathTransform(this IMathUtility mathUtils, TransformMatrix matrix)
        {
            var data = new double[]
            {
                matrix.M11 / Math.Abs(matrix.M11), matrix.M12, matrix.M13,
                matrix.M21, matrix.M22/ Math.Abs(matrix.M22), matrix.M23,
                matrix.M31, matrix.M32, matrix.M33/ Math.Abs(matrix.M33),
                matrix.M41, matrix.M42, matrix.M43,
                Math.Abs(matrix.M11),
                matrix.M14, matrix.M24, matrix.M34
            };

            return mathUtils.CreateTransform(data) as IMathTransform;
        }
    }
}