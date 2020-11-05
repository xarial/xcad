using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public interface ISwSketchSpline : ISwSketchSegment, IXSketchSpline
    {
        ISketchSpline Spline { get; }
    }

    internal class SwSketchSpline : SwSketchSegment, ISwSketchSpline
    {
        public ISketchSpline Spline => (ISketchSpline)Segment;

        public override IXPoint StartPoint => SwSelObject.FromDispatch<SwSketchPoint>((Spline.GetPoints2() as object[]).First(), m_Doc);
        public override IXPoint EndPoint => SwSelObject.FromDispatch<SwSketchPoint>((Spline.GetPoints2() as object[]).Last(), m_Doc);

        internal SwSketchSpline(ISwDocument doc, ISketchSpline spline, bool created)
            : base(doc, (ISketchSegment)spline, created)
        {
        }

        protected override ISketchSegment CreateSketchEntity()
        {
            throw new NotImplementedException();
        }
    }
}
