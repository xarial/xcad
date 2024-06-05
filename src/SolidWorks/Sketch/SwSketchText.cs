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
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public interface ISwSketchText : ISwSketchSegment, IXSketchText
    {
        ISketchText TextSegment { get; }
    }

    internal class SwSketchText : SwSketchSegment, ISwSketchText
    {
        public ISketchText TextSegment => (ISketchText)Segment;

        public override IXSketchPoint StartPoint => throw new NotSupportedException();
        public override IXSketchPoint EndPoint => throw new NotSupportedException();

        internal SwSketchText(ISketchText textSeg, SwDocument doc, SwApplication app, bool created)
            : base((ISketchSegment)textSeg, doc, app, created)
        {
        }

        internal SwSketchText(SwSketchBase ownerSketch, SwDocument doc, SwApplication app) : base(ownerSketch, doc, app)
        {
        }

        protected override ISketchSegment CreateSketchEntity()
        {
            throw new NotImplementedException();
        }
    }
}
