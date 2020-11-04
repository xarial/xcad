using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
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
        new SwCurve[] Boundary { get; set; }
    }

    internal class SwTempPlanarSurface : SwTempPrimitive, ISwTempPlanarSurface
    {
        IXSegment[] IXPlanarSurface.Boundary
        {
            get => Boundary;
            set => Boundary = value.Cast<SwCurve>().ToArray();
        }

        IXSegment[] IXRegion.Boundary => Boundary;

        internal SwTempPlanarSurface(IMathUtility mathUtils, IModeler modeler, SwTempBody body, bool isCreated)
            : base(mathUtils, modeler, body, isCreated)
        {
        }

        public SwCurve[] Boundary
        {
            get => m_Creator.CachedProperties.Get<SwCurve[]>();
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
                Plane plane = null;

                if (Boundary.FirstOrDefault()?.TryGetPlane(out plane) == true)
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

            var boundary = new List<ICurve>();

            for (int i = 0; i < Boundary.Length; i++)
            {
                boundary.AddRange(Boundary[i].Curves);

                if (i != Boundary.Length - 1)
                {
                    boundary.Add(null);
                }
            }

            var sheetBody = planarSurf.CreateTrimmedSheet4(boundary.ToArray(), true) as Body2;

            if (sheetBody == null)
            {
                throw new Exception("Failed to create profile sheet body");
            }

            return SwSelObject.FromDispatch<SwTempBody>(sheetBody);
        }
    }
}
