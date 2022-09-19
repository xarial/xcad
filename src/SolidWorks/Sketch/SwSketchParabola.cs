//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public interface ISwSketchParabola : IXSketchParabola, ISwSketchSegment
    {
        ISketchParabola Parabola { get; }
    }

    internal class SwSketchParabola : SwSketchSegment, ISwSketchParabola
    {
        public ISketchParabola Parabola => (ISketchParabola)Segment;

        public override IXSketchPoint StartPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Parabola.IGetStartPoint2());
        public override IXSketchPoint EndPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Parabola.IGetEndPoint2());

        internal SwSketchParabola(ISketchParabola parabola, SwDocument doc, SwApplication app, bool created)
            : base((ISketchSegment)parabola, doc, app, created)
        {
        }

        internal SwSketchParabola(SwSketchBase ownerSketch, SwDocument doc, SwApplication app) : base(ownerSketch, doc, app)
        {
        }

        protected override ISketchSegment CreateSketchEntity()
        {
            throw new NotImplementedException();
        }
    }
}
