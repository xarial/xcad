using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Primitives
{
    public class SwTempPlanarSurface : SwTempPrimitive, IXPlanarSurface
    {
        IXSegment IXPlanarSurface.Boundary
        {
            get => Boundary;
            set => Boundary = (SwCurve)value;
        }


        internal SwTempPlanarSurface(IMathUtility mathUtils, IModeler modeler, SwTempBody body, bool isCreated)
            : base(mathUtils, modeler, body, isCreated)
        {
        }

        public SwCurve Boundary
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

        protected override SwTempBody CreateBody()
        {
            if (Boundary.TryGetPlane(out Plane plane))
            {
                var planarSurf = m_Modeler.CreatePlanarSurface2(
                    plane.Point.ToArray(), plane.Normal.ToArray(), plane.Direction.ToArray()) as ISurface;

                if (planarSurf == null)
                {
                    throw new Exception("Failed to create plane");
                }

                var sheetBody = planarSurf.CreateTrimmedSheet4(Boundary.Curves, true) as Body2;

                if (sheetBody == null)
                {
                    throw new Exception("Failed to create profile sheet body");
                }

                return SwSelObject.FromDispatch<SwTempBody>(sheetBody);
            }
            else 
            {
                throw new Exception("Boundary is not a planar curve");
            }
        }
    }
}
