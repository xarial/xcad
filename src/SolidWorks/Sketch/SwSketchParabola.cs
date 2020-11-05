using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public interface ISwSketchParabola : IXSketchParabola
    {
        ISketchParabola Parabola { get; }
    }

    internal class SwSketchParabola : SwSketchSegment, ISwSketchParabola
    {
        public ISketchParabola Parabola => (ISketchParabola)Segment;

        public override IXPoint StartPoint => SwSelObject.FromDispatch<SwSketchPoint>(Parabola.IGetStartPoint2(), m_Doc);
        public override IXPoint EndPoint => SwSelObject.FromDispatch<SwSketchPoint>(Parabola.IGetEndPoint2(), m_Doc);

        internal SwSketchParabola(ISwDocument doc, ISketchParabola parabola, bool created)
            : base(doc, (ISketchSegment)parabola, created)
        {
        }

        protected override ISketchSegment CreateSketchEntity()
        {
            throw new NotImplementedException();
        }
    }
}
