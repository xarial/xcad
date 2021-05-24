//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Numerics;

namespace Xarial.XCad.Geometry.Structures
{
    public static class PointExtension
    {
        public static Vector3 ToVector3(this Structures.Point pt)
        {
            return new Vector3((float)pt.X, (float)pt.Y, (float)pt.Z);
        }
    }
}