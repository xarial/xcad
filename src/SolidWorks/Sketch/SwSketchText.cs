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
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public interface ISwSketchText : ISwSketchSegment, IXSketchText
    {
        ISketchText TextSegment { get; }
    }

    internal class SwSketchText : SwSketchSegment, ISwSketchText
    {
        public ISketchText TextSegment => (ISketchText)Segment;

        public override IXPoint StartPoint => throw new NotSupportedException();
        public override IXPoint EndPoint => throw new NotSupportedException();

        internal SwSketchText(ISketchText textSeg, ISwDocument doc, ISwApplication app, bool created)
            : base((ISketchSegment)textSeg, doc, app, created)
        {
        }

        protected override ISketchSegment CreateSketchEntity()
        {
            throw new NotImplementedException();
        }
    }
}
