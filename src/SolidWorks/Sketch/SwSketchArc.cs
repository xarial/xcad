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
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public interface ISwSketchArc : IXSketchArc 
    {
        ISketchArc Arc { get; }
    }

    internal class SwSketchArc : SwSketchSegment, ISwSketchArc
    {
        public ISketchArc Arc => (ISketchArc)Segment;

        public override IXPoint StartPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Arc.IGetStartPoint2());
        public override IXPoint EndPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Arc.IGetEndPoint2());

        public double Diameter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Point Center { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Vector Axis { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        internal SwSketchArc(ISketchArc arc, ISwDocument doc, ISwApplication app, bool created)
            : base((ISketchSegment)arc, doc, app, created)
        {
        }

        protected override ISketchSegment CreateSketchEntity()
        {
            throw new NotImplementedException();
        }
    }
}
