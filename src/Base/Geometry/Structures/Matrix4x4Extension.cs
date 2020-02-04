//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Numerics;

namespace Xarial.XCad.Geometry.Structures
{
    public static class Matrix4x4Extension
    {
        public static TransformMatrix ToTransformMatrix(this Matrix4x4 matrix)
        {
            return new TransformMatrix(
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44);
        }
    }
}