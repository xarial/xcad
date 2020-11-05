using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public class SwSketchEllipse : SwSketchSegment, IXSketchEllipse
    {
        public ISketchEllipse Ellipse => (ISketchEllipse)Segment;

        public override IXPoint StartPoint => SwSelObject.FromDispatch<SwSketchPoint>(Ellipse.IGetStartPoint2(), m_Doc);
        public override IXPoint EndPoint => SwSelObject.FromDispatch<SwSketchPoint>(Ellipse.IGetEndPoint2(), m_Doc);
        
        internal SwSketchEllipse(ISwDocument doc, ISketchEllipse ellipse, bool created)
            : base(doc, (ISketchSegment)ellipse, created)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }
        }

        protected override ISketchSegment CreateSketchEntity()
        {
            throw new NotImplementedException();
        }
    }
}
