//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Geometry.Curves;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public interface ISwSketchRegion : IXSketchRegion, ISwRegion, ISwSelObject
    {
        ISketchRegion Region { get; }
    }

    internal class SwSketchRegion : SwSelObject, ISwSketchRegion
    {
        IXSegment[] IXRegion.Boundary => Boundary;

        internal SwSketchRegion(ISketchRegion region, ISwDocument doc) : base(region, doc)
        {
            Region = region;
        }

        public ISketchRegion Region { get; }

        public Plane Plane => throw new NotImplementedException();
        
        public ISwCurve[] Boundary => (Region.GetEdges() as object[])
                                        .Cast<IEdge>()
                                        .Select(e => SwObject.FromDispatch<ISwCurve>(e.IGetCurve()))
                                        .ToArray();
    }
}
