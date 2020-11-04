using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Primitives
{
    public interface ISwTempPlanarSurface : IXPlanarSurface, ISwTempRegion, ISwTempPrimitive
    {
        new SwCurve Boundary { get; set; }
    }

    internal class SwTempPlanarSurface : SwTempPrimitive, ISwTempPlanarSurface
    {
        IXSegment IXPlanarSurface.Boundary
        {
            get => Boundary;
            set => Boundary = (SwCurve)value;
        }

        IXSegment IXRegion.Boundary => Boundary;

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

        public Plane Plane 
        {
            get 
            {
                if (Boundary.TryGetPlane(out Plane plane))
                {
                    return plane;
                }
                else 
                {
                    throw new Exception("Boundary is not a planar curve");
                }
            }
        }

        protected override SwTempBody CreateBody()
        {
            var plane = Plane;

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
    }
}
