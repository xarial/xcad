//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Surfaces;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Surfaces;

namespace Xarial.XCad.SolidWorks.Geometry.Surfaces
{
    public interface ISwSphericalSurface : ISwSurface, IXSphericalSurface
    {
    }

    internal class SwSphericalSurface : SwSurface, ISwSphericalSurface
    {
        public SwSphericalSurface(ISurface surface, SwDocument doc, SwApplication app) : base(surface, doc, app)
        {
        }

        public double Radius
        {
            get 
            {
                var sphereParams = (double[])Surface.SphereParams;
                return sphereParams[3];
            }
        }

        public Point Origin
        {
            get
            {
                var sphereParams = (double[])Surface.SphereParams;

                return new Point(sphereParams[0], sphereParams[1], sphereParams[2]);
            }
        }
    }
}
