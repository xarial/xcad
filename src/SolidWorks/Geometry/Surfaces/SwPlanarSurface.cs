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

namespace Xarial.XCad.SolidWorks.Geometry.Surfaces
{
    public interface ISwPlanarSurface : ISwSurface, IXPlanarSurface 
    {
    }

    internal class SwPlanarSurface : SwSurface, ISwPlanarSurface
    {
        internal SwPlanarSurface(ISurface surface) : base(surface)
        {
        }

        public Plane Plane 
        {
            get 
            {
                var planeParams = Surface.PlaneParams as double[];
                
                var rootPt = new Point(planeParams[3], planeParams[4], planeParams[5]);
                var normVec = new Vector(planeParams[0], planeParams[1], planeParams[2]);
                var refVec = normVec.CreateAnyPerpendicular();

                return new Plane(rootPt, normVec, refVec);
            }
        }
    }
}
