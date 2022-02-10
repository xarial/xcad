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

namespace Xarial.XCad.SolidWorks.Sketch
{
    public interface ISwSketchEllipse : IXSketchEllipse
    {
        ISketchEllipse Ellipse { get; }
    }

    internal class SwSketchEllipse : SwSketchSegment, ISwSketchEllipse
    {
        public ISketchEllipse Ellipse => (ISketchEllipse)Segment;

        public override IXSketchPoint StartPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Ellipse.IGetStartPoint2());
        public override IXSketchPoint EndPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Ellipse.IGetEndPoint2());
        
        internal SwSketchEllipse(ISketchEllipse ellipse, ISwDocument doc, ISwApplication app, bool created)
            : base((ISketchSegment)ellipse, doc, app, created)
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
