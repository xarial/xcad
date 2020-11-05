//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Curves
{
    public interface ISwPolylineCurve : ISwCurve, IXPolylineCurve
    {
    }

    internal class SwPolylineCurve : SwCurve, ISwPolylineCurve
    {
        internal SwPolylineCurve(IModeler modeler, ICurve[] curves, bool isCreated) 
            : base(modeler, curves, isCreated)
        {
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

        public override bool TryGetPlane(out Plane plane)
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

        protected override ICurve[] Create()
        {
            var retVal = new ICurve[Points.Length - 1];

            for (int i = 1; i < Points.Length; i++) 
            {
                retVal[i - 1] = CreateLine(Points[i - 1], Points[i]);
            }

            return retVal;
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
