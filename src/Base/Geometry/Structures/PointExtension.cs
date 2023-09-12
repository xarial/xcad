//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.Geometry.Structures
{
    /// <summary>
    /// Additional methods for the <see cref="Point"/> class
    /// </summary>
    public static class PointExtension 
    {
        /// <summary>
        /// Projects this point onto the plane
        /// </summary>
        /// <param name="pt">Point to project</param>
        /// <param name="plane">Plane to project to</param>
        /// <returns>New projected point</returns>
        public static Point Project(this Point pt, Plane plane)
        {
            var a = plane.Normal.X;
            var b = plane.Normal.Y;
            var c = plane.Normal.Z;

            var p = plane.Point.X;
            var q = plane.Point.Y;
            var r = plane.Point.Z;

            var d = a * p + b * q + c * r;

            var z1 = pt.X;
            var z2 = pt.Y;
            var z3 = pt.Z;

            var k = (d - a * z1 - b * z2 - c * z3) / (Math.Pow(a, 2) + Math.Pow(b, 2) + Math.Pow(c, 2));

            return new Point(z1 + k * a, z2 + k * b, z3 + k * c);
        }
    }
}