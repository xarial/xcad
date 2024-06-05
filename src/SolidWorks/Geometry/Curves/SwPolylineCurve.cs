﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
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
    public interface ISwPolylineCurve : ISwCurve, IXPolylineCurve
    {
    }

    internal class SwPolylineCurve : SwCurve, ISwPolylineCurve
    {
        internal SwPolylineCurve(ICurve[] curves, SwDocument doc, SwApplication app, bool isCreated) 
            : base(curves, doc, app, isCreated)
        {
            m_Creator.CachedProperties.Set(PolylineMode_e.Strip, nameof(Mode));
        }

        public PolylineMode_e Mode
        {
            get => m_Creator.CachedProperties.Get<PolylineMode_e>();
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedSegmentReadOnlyParameterException();
                }
            }
        }

        public Point[] Points
        {
            get => m_Creator.CachedProperties.Get<Point[]>();
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedSegmentReadOnlyParameterException();
                }
            }
        }

        internal override bool TryGetPlane(out Plane plane)
        {
            if (Points?.Length > 2)
            {
                //TODO: validate that all points are on the plane
                plane = new Plane(Points.First(),
                    (Points[1] - Points[0]).Cross(Points[2] - Points[1]),
                    Points[1] - Points[0]);

                return true;
            }
            else 
            {
                plane = null;
                return false;
            }
        }

        protected override ICurve[] Create(CancellationToken cancellationToken)
        {
            if (Points.Length < 2) 
            {
                throw new Exception("it must be 2 or more points defined in the polyline");
            }

            ICurve[] retVal;

            switch (Mode)
            {
                case PolylineMode_e.Lines:
                    if (Points.Length % 2 != 0) 
                    {
                        throw new Exception("Number of points must be even in the Lines mode of polyline curve");
                    }

                    retVal = new ICurve[(int)(Points.Length / 2)];

                    for (int i = 0; i < retVal.Length; i++) 
                    {
                        retVal[i] = CreateLine(Points[i * 2], Points[i * 2 + 1]);
                    }
                    break;

                case PolylineMode_e.Strip:
                    retVal = new ICurve[Points.Length - 1];

                    for (int i = 1; i < Points.Length; i++)
                    {
                        retVal[i - 1] = CreateLine(Points[i - 1], Points[i]);
                    }
                    break;

                case PolylineMode_e.Loop:
                    retVal = new ICurve[Points.Length > 2 ? Points.Length : Points.Length - 1];

                    for (int i = 1; i < Points.Length; i++)
                    {
                        retVal[i - 1] = CreateLine(Points[i - 1], Points[i]);
                    }

                    if (Points.Length > 2) 
                    {
                        retVal[retVal.Length - 1] = CreateLine(Points[Points.Length - 1], Points[0]);
                    }

                    break;

                default:
                    throw new NotSupportedException("Not supported mode of the polyline");
            }

            return retVal;
        }

        public override IXCurve Copy()
        {
            var copies = Curves.Select(c => c.ICopy()).ToArray();
            var polyCurve = new SwPolylineCurve(copies, OwnerDocument, OwnerApplication, true);

            polyCurve.m_Creator.CachedProperties.Set(Mode);
            polyCurve.m_Creator.CachedProperties.Set(Points);

            return polyCurve;
        }

        private ICurve CreateLine(Point startPt, Point endPt)
        {
            var line = m_Modeler.CreateLine(startPt.ToArray(), (startPt - endPt).ToArray()) as ICurve;
            line = line.CreateTrimmedCurve2(startPt.X, startPt.Y, startPt.Z, endPt.X, endPt.Y, endPt.Z);

            if (line == null)
            {
                throw new NullReferenceException("Failed to create line");
            }

            return line;
        }
    }
}
