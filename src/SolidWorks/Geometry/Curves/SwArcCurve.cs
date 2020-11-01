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
    public class SwArcCurve : SwCurve, IXArcCurve
    {
        internal SwArcCurve(IModeler modeler, ICurve curve, bool isCreated) : base(modeler, new ICurve[] { curve }, isCreated)
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

        internal override bool TryGetPlane(out Plane plane)
        {
            plane = new Plane(Center, Axis, ReferenceDirection);
            return true;
        }
        
        private Vector ReferenceDirection 
        {
            get 
            {
                Vector refDir;
                var zVec = new Vector(0, 0, 1);

                if (Axis.IsSame(zVec))
                {
                    refDir = new Vector(1, 0, 0);
                }
                else
                {
                    refDir = Axis.Cross(zVec);
                }

                return refDir;
            }
        }

        protected override ICurve[] Create()
        {
            //TODO: check if this is not closed arc

            var refPt = Center.Move(ReferenceDirection, Diameter / 2);

            var arc = m_Modeler.CreateArc(Center.ToArray(), Axis.ToArray(), Diameter / 2, refPt.ToArray(), refPt.ToArray()) as ICurve;
            arc = arc.CreateTrimmedCurve2(refPt.X, refPt.Y, refPt.Z, refPt.X, refPt.Y, refPt.Z);

            return new ICurve[] { arc };
        }

    }
}
