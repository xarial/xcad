//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Geometry.Structures
{
    public class PrincipalAxesOfInertia
    {
        public Vector Ix { get; }
        public Vector Iy { get; }
        public Vector Iz { get; }

        public PrincipalAxesOfInertia(Vector ix, Vector iy, Vector iz)
        {
            Ix = ix;
            Iy = iy;
            Iz = iz;
        }
    }
}
