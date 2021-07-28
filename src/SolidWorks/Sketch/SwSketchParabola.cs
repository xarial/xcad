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
    public interface ISwSketchParabola : IXSketchParabola
    {
        ISketchParabola Parabola { get; }
    }

    internal class SwSketchParabola : SwSketchSegment, ISwSketchParabola
    {
        public ISketchParabola Parabola => (ISketchParabola)Segment;

        public override IXPoint StartPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Parabola.IGetStartPoint2());
        public override IXPoint EndPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Parabola.IGetEndPoint2());

        internal SwSketchParabola(ISketchParabola parabola, ISwDocument doc, ISwApplication app, bool created)
            : base((ISketchSegment)parabola, doc, app, created)
        {
        }

        protected override ISketchSegment CreateSketchEntity()
        {
            throw new NotImplementedException();
        }
    }
}
