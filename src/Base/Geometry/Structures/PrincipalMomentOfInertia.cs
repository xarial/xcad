﻿//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Geometry.Structures
{
    public class PrincipalMomentOfInertia
    {
        public double Px { get; }
        public double Py { get; }
        public double Pz { get; }

        public PrincipalMomentOfInertia(double px, double py, double pz)
        {
            Px = px;
            Py = py;
            Pz = pz;
        }

        public PrincipalMomentOfInertia(double[] p) : this(p[0], p[1], p[2])
        {
        }
    }
}
