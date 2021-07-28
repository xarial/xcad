//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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
        internal SwCylindricalSurface(ISurface surface, ISwDocument doc, ISwApplication app) : base(surface, doc, app)
        {
        }

        public Point Origin
        {
            get
            {
                var cylParams = CylinderParams;

                return new Point(cylParams[0], cylParams[1], cylParams[2]);
            }
        }

        public Vector Axis
        {
            get
            {
                var cylParams = CylinderParams;

                return new Vector(cylParams[3], cylParams[4], cylParams[5]);
            }
        }

        public double Radius => CylinderParams[6];

        private double[] CylinderParams
        {
            get
            {
                return Surface.CylinderParams as double[];
            }
        }
    }
}
