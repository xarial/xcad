//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Numerics;

namespace Xarial.XCad.Geometry.Structures
{
    public static class TransformMatrixExtension
    {
        public static Matrix4x4 ToMatrix4x4(this TransformMatrix transform)
        {
            return new Matrix4x4(
                (float)transform.M11, (float)transform.M12, (float)transform.M13, (float)transform.M14,
                (float)transform.M21, (float)transform.M22, (float)transform.M23, (float)transform.M24,
                (float)transform.M31, (float)transform.M32, (float)transform.M33, (float)transform.M34,
                (float)transform.M41, (float)transform.M42, (float)transform.M43, (float)transform.M44);
        }
    }
}