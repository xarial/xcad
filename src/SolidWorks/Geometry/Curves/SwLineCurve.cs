//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Curves
{
    public interface ISwLineCurve : IXLineCurve, ISwCurve
    {
    }

    internal class SwLineCurve : SwCurve, ISwLineCurve
    {
        internal SwLineCurve(ICurve curve, SwDocument doc, SwApplication app, bool isCreated) 
            : base(curve, doc, app, isCreated)
        {
        }

        public Line Geometry
        {
            get
            {
                if (IsCommitted)
                {
                    return new Line(StartPoint.Coordinate, EndPoint.Coordinate);
                }
                else
                {
                    return m_Creator.CachedProperties.Get<Line>();
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

        protected override ICurve[] Create(CancellationToken cancellationToken)
        {
            var geom = Geometry;

            var line = m_Modeler.CreateLine(geom.StartPoint.ToArray(), (geom.StartPoint - geom.EndPoint).ToArray()) as ICurve;
            line = line.CreateTrimmedCurve2(geom.StartPoint.X, geom.StartPoint.Y, geom.StartPoint.Z, geom.EndPoint.X, geom.EndPoint.Y, geom.EndPoint.Z);

            if (line == null)
            {
                throw new NullReferenceException("Failed to create line");
            }

            return new ICurve[] { line };
        }
    }
}
