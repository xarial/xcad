//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Sketch;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public class SwSketchLine : SwSketchEntity<ISketchLine>, IXSketchLine
    {
        private readonly SwSketchPoint m_StartPoint;
        private readonly SwSketchPoint m_EndPoint;

        public IXPoint StartPoint => m_StartPoint;
        public IXPoint EndPoint => m_EndPoint;

        internal SwSketchLine(IModelDoc2 model, ISketchLine ent, bool created) : base(model, ent, created)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            m_StartPoint = new SwSketchPoint(model, ent?.IGetStartPoint2(), created);
            m_EndPoint = new SwSketchPoint(model, ent?.IGetEndPoint2(), created);
        }

        protected override ISketchLine CreateSketchEntity()
        {
            var line = (ISketchLine)m_SketchMgr.CreateLine(
                StartPoint.Coordinate.X,
                StartPoint.Coordinate.Y,
                StartPoint.Coordinate.Z,
                EndPoint.Coordinate.X,
                EndPoint.Coordinate.Y,
                EndPoint.Coordinate.Z);

            m_StartPoint.SetLinePoint(line.IGetStartPoint2());
            m_EndPoint.SetLinePoint(line.IGetEndPoint2());

            return line;
        }
    }
}