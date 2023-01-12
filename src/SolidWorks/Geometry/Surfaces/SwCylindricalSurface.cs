//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Surfaces;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Geometry.Surfaces
{
    public interface ISwCylindricalSurface : ISwSurface, IXCylindricalSurface 
    {
    }

    internal class SwCylindricalSurface : SwSurface, ISwCylindricalSurface
    {
        internal SwCylindricalSurface(ISurface surface, SwDocument doc, SwApplication app) : base(surface, doc, app)
        {
        }

        public Axis Axis
        {
            get
            {
                var cylParams = CylinderParams;

                return new Axis(
                    new Point(cylParams[0], cylParams[1], cylParams[2]),
                    new Vector(cylParams[3], cylParams[4], cylParams[5]));
            }
        }

        public double Radius => CylinderParams[6];

        private double[] CylinderParams => Surface.CylinderParams as double[];
    }
}
