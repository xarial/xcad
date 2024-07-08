//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public interface ISwSketchCircle : IXSketchCircle, ISwSketchSegment
    {
        ISketchArc Arc { get; }
    }

    public interface ISwSketchArc : ISwSketchCircle, IXSketchArc
    {
    }

    internal class SwSketchCircle : SwSketchSegment, ISwSketchCircle
    {
        public ISketchArc Arc => (ISketchArc)Segment;

        public override IXSketchPoint StartPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Arc.IGetStartPoint2());
        public override IXSketchPoint EndPoint => OwnerDocument.CreateObjectFromDispatch<SwSketchPoint>(Arc.IGetEndPoint2());

        public Circle Geometry 
        {
            get 
            {
                if (IsCommitted)
                {
                    var centerPt = CreatePoint((ISketchPoint)Arc.GetCenterPoint2());
                    var diam = Arc.GetRadius() * 2;

                    var norm = (double[])Arc.GetNormalVector();

                    return new Circle(new Axis(centerPt, new Vector(norm)), diam);
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<Circle>();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    Arc.SetRadius(value.Diameter / 2);
                    SetPoint((ISketchPoint)Arc.GetCenterPoint2(), value.CenterAxis.Point);
                    //TODO: implement changing of the axis
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        internal SwSketchCircle(ISketchArc arc, SwDocument doc, SwApplication app, bool created)
            : base((ISketchSegment)arc, doc, app, created)
        {
        }

        internal SwSketchCircle(SwSketchBase ownerSketch, SwDocument doc, SwApplication app) : base(ownerSketch, doc, app)
        {
        }

        protected override ISketchSegment CreateSketchEntity()
        {
            var geom = Geometry;

            var centerPt = geom.CenterAxis.Point;

            return m_SketchMgr.CreateCircleByRadius(centerPt.X, centerPt.Y, centerPt.Z, geom.Diameter / 2);
        }

        protected Point CreatePoint(ISketchPoint pt) => new Point(pt.X, pt.Y, pt.Z);

        protected void SetPoint(ISketchPoint pt, Point coord) 
        {
            pt.X = coord.X;
            pt.Y = coord.Y;
            pt.Z = coord.Z;
        }
    }

    internal class SwSketchArc : SwSketchCircle, ISwSketchArc
    {
        public Point Start
        {
            get
            {
                if (IsCommitted)
                {
                    return CreatePoint((ISketchPoint)Arc.GetStartPoint2());
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<Point>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    SetPoint((ISketchPoint)Arc.GetStartPoint2(), value);
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public Point End
        {
            get
            {
                if (IsCommitted)
                {
                    return CreatePoint((ISketchPoint)Arc.GetEndPoint2());
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<Point>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    SetPoint((ISketchPoint)Arc.GetEndPoint2(), value);
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        internal SwSketchArc(ISketchArc arc, SwDocument doc, SwApplication app, bool created) : base(arc, doc, app, created)
        {
        }

        internal SwSketchArc(SwSketchBase ownerSketch, SwDocument doc, SwApplication app) : base(ownerSketch, doc, app)
        {
        }

        protected override ISketchSegment CreateSketchEntity()
        {
            const int DIR_CLOCKWICE = -1;

            if (Start == null)
            {
                throw new NullReferenceException("Start point coordinate is not specified");
            }

            if (End == null)
            {
                throw new NullReferenceException("End point coordinate is not specified");
            }

            var geom = Geometry;
            var centerPt = geom.CenterAxis.Point;

            return m_SketchMgr.CreateArc(centerPt.X, centerPt.Y, centerPt.Z, Start.X, Start.Y, Start.Z, End.X, End.Y, End.Z, DIR_CLOCKWICE);
        }
    }
}
