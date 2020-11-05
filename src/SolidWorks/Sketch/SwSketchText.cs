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

        internal SwSketchText(ISwDocument doc, ISketchText textSeg, bool created)
            : base(doc, (ISketchSegment)textSeg, created)
        {
        }

        protected override ISketchSegment CreateSketchEntity()
        {
            throw new NotImplementedException();
        }
    }
}
