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
    public interface ISwSketchCircle : IXSketchCircle 
    {
        ISketchArc Arc { get; }
    }

    public interface ISwSketchArc : ISwSketchCircle, IXSketchArc
    {
    }

    internal class SwSketchCircle : SwSketchSegment, ISwSketchCircle
    {
        public ISketchArc Arc => (ISketchArc)Segment;

        public override IXSketchPoint StartPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Arc.IGetStartPoint2());
        public override IXSketchPoint EndPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Arc.IGetEndPoint2());

        public double Diameter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Point Center { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Vector Axis { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        internal SwSketchCircle(ISketchArc arc, ISwDocument doc, ISwApplication app, bool created)
            : base((ISketchSegment)arc, doc, app, created)
        {
        }

        protected override ISketchSegment CreateSketchEntity()
        {
            throw new NotImplementedException();
        }
    }

    internal class SwSketchArc : SwSketchCircle, ISwSketchArc
    {
        public Point Start { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Point End { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        internal SwSketchArc(ISketchArc arc, ISwDocument doc, ISwApplication app, bool created) : base(arc, doc, app, created)
        {
        }
    }
}
