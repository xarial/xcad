//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public class SwSketchLine : SwSketchEntity<ISketchLine>, IXSketchLine
    {
        private SwSketchPoint m_StartPoint;
        private SwSketchPoint m_EndPoint;

        public IXPoint StartPoint
        {
            get 
            {
                if (IsCommitted)
                {
                    return m_StartPoint;
                }
                else 
                {
                    throw new Exception("This property is only available for committed entity");
                }
            }
        }

        public IXPoint EndPoint 
        {
            get
            {
                if (IsCommitted)
                {
                    return m_StartPoint;
                }
                else
                {
                    throw new Exception("This property is only available for committed entity");
                }
            }
        }

        private Point m_CachedStartCoordinate;
        private Point m_CachedEndCoordinate;

        public Point StartCoordinate 
        {
            get 
            {
                if (IsCommitted)
                {
                    return StartPoint.Coordinate;
                }
                else 
                {
                    return m_CachedStartCoordinate;
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    StartPoint.Coordinate = value;
                }
                else 
                {
                    m_CachedStartCoordinate = value;
                }
            }
        }
        
        public Point EndCoordinate 
        {
            get
            {
                if (IsCommitted)
                {
                    return EndPoint.Coordinate;
                }
                else
                {
                    return m_CachedEndCoordinate;
                }
            }
            set
            {
                if (IsCommitted)
                {
                    EndPoint.Coordinate = value;
                }
                else
                {
                    m_CachedEndCoordinate = value;
                }
            }
        }

        internal SwSketchLine(SwDocument doc, ISketchLine ent, bool created) : base(doc, ent, created)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            if (IsCommitted)
            {
                m_StartPoint = SwSelObject.FromDispatch<SwSketchPoint>(ent.IGetStartPoint2(), doc);
                m_EndPoint = SwSelObject.FromDispatch<SwSketchPoint>(ent.IGetEndPoint2(), doc);
            }
        }

        protected override ISketchLine CreateSketchEntity()
        {
            var line = (ISketchLine)m_SketchMgr.CreateLine(
                StartCoordinate.X,
                StartCoordinate.Y,
                StartCoordinate.Z,
                EndCoordinate.X,
                EndCoordinate.Y,
                EndCoordinate.Z);

            m_StartPoint = SwSelObject.FromDispatch<SwSketchPoint>(line.IGetStartPoint2(), m_Doc);
            m_EndPoint = SwSelObject.FromDispatch<SwSketchPoint>(line.IGetEndPoint2(), m_Doc);
            
            return line;
        }
    }
}