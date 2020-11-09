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
using System.Threading;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Curves
{
    public interface ISwArcCurve : IXArcCurve 
    {
    }

    internal class SwArcCurve : SwCurve, ISwArcCurve
    {
        internal SwArcCurve(IModeler modeler, ICurve curve, bool isCreated) 
            : base(modeler, new ICurve[] { curve }, isCreated)
        {
        }

        public double Diameter 
        {
            get 
            {
                if (IsCommitted)
                {
                    var circParams = Curves.First().CircleParams as double[];
                    return circParams[6];
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

        public override bool TryGetPlane(out Plane plane)
        {
            plane = new Plane(Center, Axis, ReferenceDirection);
            return true;
        }

        private Vector ReferenceDirection => Axis.CreateAnyPerpendicular();

        protected override ICurve[] Create(CancellationToken cancellationToken)
        {
            //TODO: check if this is not closed arc

            var refPt = Center.Move(ReferenceDirection, Diameter / 2);

            var arc = m_Modeler.CreateArc(Center.ToArray(), Axis.ToArray(), Diameter / 2, refPt.ToArray(), refPt.ToArray()) as ICurve;
            arc = arc.CreateTrimmedCurve2(refPt.X, refPt.Y, refPt.Z, refPt.X, refPt.Y, refPt.Z);

            if (arc == null) 
            {
                throw new NullReferenceException("Failed to create arc");
            }

            return new ICurve[] { arc };
        }

    }
}
