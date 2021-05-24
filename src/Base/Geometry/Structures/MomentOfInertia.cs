//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Geometry.Structures
{
    public class MomentOfInertia
    {
        public Vector Lx { get; }
        public Vector Ly { get; }
        public Vector Lz { get; }

        public MomentOfInertia(Vector lx, Vector ly, Vector lz)
        {
            Lx = lx;
            Ly = ly;
            Lz = lz;
        }
    }
}
