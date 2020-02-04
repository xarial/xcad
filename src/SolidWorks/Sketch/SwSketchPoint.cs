//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Sketch;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public class SwSketchPoint : SwSketchEntity<ISketchPoint>, IXSketchPoint
    {
        private Point m_CachedCoordinate;
        private ISketchPoint m_LinePoint;

        public SwSketchPoint(IModelDoc2 model, ISketchPoint ent, bool created) : base(model, ent, created)
        {
        }

        public Point Coordinate
        {
            get
            {
                if (m_Creator.IsCreated)
                {
                    return new Point(Element.X, Element.Y, Element.Z);
                }
                else
                {
                    return m_CachedCoordinate ?? (m_CachedCoordinate = new Point(0, 0, 0));
                }
            }
            set
            {
                if (m_Creator.IsCreated)
                {
                    if (m_SketchMgr.ActiveSketch != Element.GetSketch())
                    {
                        throw new Exception("You must set the sketch into editing mode in order to modify the cooridinate");
                    }

                    Element.SetCoords(value.X, value.Y, value.Z);
                }
                else
                {
                    m_CachedCoordinate = value;
                }
            }
        }

        protected override ISketchPoint CreateSketchEntity()
        {
            if (m_LinePoint == null)
            {
                return m_SketchMgr.CreatePoint(Coordinate.X, Coordinate.Y, Coordinate.Z);
            }
            else
            {
                return m_LinePoint;
            }
        }

        internal void SetLinePoint(ISketchPoint pt)
        {
            m_LinePoint = pt;
            Create();
        }
    }
}