//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Curves
{
    public interface ISwCircleCurve : IXCircleCurve, ISwCurve
    {
    }

    public interface ISwArcCurve : ISwCircleCurve, IXArcCurve
    {
    }

    internal class SwCircleCurve : SwCurve, ISwCircleCurve
    {
        internal SwCircleCurve(ICurve curve, SwDocument doc, SwApplication app, bool isCreated) 
            : base(new ICurve[] { curve }, doc, app, isCreated)
        {
        }

        public double Diameter 
        {
            get 
            {
                if (IsCommitted)
                {
                    var circParams = Curves.First().CircleParams as double[];
                    return circParams[6] * 2;
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<double>();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    throw new CommitedSegmentReadOnlyParameterException();
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }
        
        public Point Center 
        {
            get
            {
                if (IsCommitted)
                {
                    var circParams = Curves.First().CircleParams as double[];
                    return new Point(circParams[0], circParams[1], circParams[2]);
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
                    throw new CommitedSegmentReadOnlyParameterException();
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public Vector Axis
        {
            get
            {
                if (IsCommitted)
                {
                    var circParams = Curves.First().CircleParams as double[];
                    return new Vector(circParams[3], circParams[4], circParams[5]);
                }
                else
                {
                    return m_Creator.CachedProperties.Get<Vector>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    throw new CommitedSegmentReadOnlyParameterException();
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        internal override bool TryGetPlane(out Plane plane)
        {
            plane = new Plane(Center, Axis, ReferenceDirection);
            return true;
        }

        private Vector ReferenceDirection => Axis.CreateAnyPerpendicular();

        protected virtual void GetEndPoints(out Point start, out Point end) 
        {
            start = Center.Move(ReferenceDirection, Diameter / 2);
            end = start;
        }

        protected override ICurve[] Create(CancellationToken cancellationToken)
        {
            GetEndPoints(out Point start, out Point end);

            var arc = m_Modeler.CreateArc(Center.ToArray(), Axis.ToArray(), Diameter / 2, start.ToArray(), end.ToArray()) as ICurve;

            if (arc == null) 
            {
                throw new Exception("Failed to create arc");
            }

            arc = arc.CreateTrimmedCurve2(start.X, start.Y, start.Z, end.X, end.Y, end.Z);

            if (arc == null) 
            {
                throw new NullReferenceException("Failed to trim arc");
            }

            return new ICurve[] { arc };
        }

    }

    internal class SwArcCurve : SwCircleCurve, ISwArcCurve
    {
        internal SwArcCurve(ICurve curve, SwDocument doc, SwApplication app, bool isCreated) : base(curve, doc, app, isCreated)
        {
        }

        public Point Start
        {
            get
            {
                if (IsCommitted)
                {
                    return StartPoint.Coordinate;
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
                    throw new Exception("Cannot change the start point after the curve is created");
                }
                else
                {
                    m_Creator.CachedProperties.Set<Point>(value);
                }
            }
        }

        public Point End
        {
            get
            {
                if (IsCommitted)
                {
                    return EndPoint.Coordinate;
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
                    throw new Exception("Cannot change the start point after the curve is created");
                }
                else
                {
                    m_Creator.CachedProperties.Set<Point>(value);
                }
            }
        }

        protected override void GetEndPoints(out Point start, out Point end)
        {
            if (Start == null || End == null)
            {
                throw new Exception("Start or End point is not specified");
            }

            start = Start;
            end = End;
        }
    }
}
