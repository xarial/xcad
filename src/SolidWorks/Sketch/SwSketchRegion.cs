//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public interface ISwSketchRegion : IXSketchRegion, ISwPlanarRegion, ISwSelObject
    {
        ISketchRegion Region { get; }
    }

    internal class SwSketchRegion : SwSelObject, ISwSketchRegion
    {
        IXSegment[] IXRegion.Boundary => Boundary;

        internal SwSketchRegion(ISketchRegion region, SwDocument doc, SwApplication app) : base(region, doc, app)
        {
            Region = region;
        }

        public ISketchRegion Region { get; }

        public Plane Plane
        {
            get 
            {
                var transform = Region.Sketch.ModelToSketchTransform.IInverse().ToTransformMatrix();

                var x = new Vector(1, 0, 0).Transform(transform);
                var z = new Vector(0, 0, 1).Transform(transform);
                var origin = new Point(0, 0, 0).Transform(transform);

                return new Plane(origin, z, x);
            }
        }
        
        public ISwCurve[] Boundary => (Region.GetEdges() as object[])
            .Cast<IEdge>()
            .Select(e => OwnerApplication.CreateObjectFromDispatch<ISwCurve>(e.IGetCurve().ICopy(), OwnerDocument))
            .ToArray();

        public ISwTempPlanarSheetBody PlanarSheetBody => this.ToPlanarSheetBody(OwnerApplication.MemoryGeometryBuilder);
    }
}
