using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Primitives
{
    public class SwTempRevolve : SwTempPrimitive, IXRevolve
    {
        internal SwTempRevolve(IMathUtility mathUtils, IModeler modeler, SwTempBody body, bool isCreated) 
            : base(mathUtils, modeler, body, isCreated)
        {
        }

        IXSegment IXRevolve.Profile 
        {
            get => Profile;
            set => Profile = (SwCurve)value; 
        }

        IXLine IXRevolve.Axis
        {
            get => Axis;
            set => Axis = (SwLineCurve)value;
        }
                
        public double Angle
        {
            get => m_Creator.CachedProperties.Get<double>();
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

        public SwCurve Profile
        {
            get => m_Creator.CachedProperties.Get<SwCurve>();
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

        public SwLineCurve Axis
        {
            get => m_Creator.CachedProperties.Get<SwLineCurve>();
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

        protected override SwTempBody CreateBody()
        {
            return base.CreateBody();
        }
    }
}
