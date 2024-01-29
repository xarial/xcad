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
    public interface ISwToroidalSurface : ISwSurface, IXToroidalSurface
    {
    }

    internal class SwToroidalSurface : SwSurface, ISwToroidalSurface
    {
        public SwToroidalSurface(ISurface surface, SwDocument doc, SwApplication app) : base(surface, doc, app)
        {
        }

        public Axis Axis 
        {
            get 
            {
                var torParams = (double[])Surface.TorusParams;

                return new Axis(
                    new Point(torParams[0], torParams[1], torParams[2]),
                    new Vector(torParams[3], torParams[4], torParams[5]));
            }
        }

        public double MajorRadius => TorusParams[6];

        public double MinorRadius => TorusParams[7];

        private double[] TorusParams => (double[])Surface.TorusParams;
    }
}
