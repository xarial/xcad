using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Primitives
{
    public interface ISwTempExtrusion : IXExtrusion, ISwTempPrimitive
    {
        new ISwTempRegion[] Profiles { get; set; }
    }

    internal class SwTempExtrusion : SwTempPrimitive, ISwTempExtrusion
    {
        IXRegion[] IXExtrusion.Profiles
        {
            get => Profiles;
            set => Profiles = value?.Cast<ISwTempRegion>().ToArray();
        }

        public double Depth
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

        public Vector Direction
        {
            get => m_Creator.CachedProperties.Get<Vector>();
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

        public ISwTempRegion[] Profiles
        {
            get => m_Creator.CachedProperties.Get<ISwTempRegion[]>();
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

        internal SwTempExtrusion(IMathUtility mathUtils, IModeler modeler, SwTempBody body, bool isCreated) 
            : base(mathUtils, modeler, body, isCreated)
        {
        }

        protected override SwTempBody CreateBody()
        {
            var plane = Profiles.First().Plane;

            var surf = CreatePlanarSurface(plane.Point, plane.Normal, plane.Direction);

            var dir = m_MathUtils.CreateVector(Direction.ToArray()) as MathVector;

            var boundary = new List<ICurve>();

            for (int i = 0; i < Profiles.Length; i++) 
            {
                boundary.AddRange(Profiles[i].Boundary.Curves);

                if (i != Profiles.Length - 1) 
                {
                    boundary.Add(null);
                }
            }

            var body = Extrude(surf, boundary.ToArray(), dir, Depth);

            if (body == null) 
            {
                throw new Exception("Failed to create extrusion");
            }

            return SwSelObject.FromDispatch<SwTempBody>(body);
        }

        private ISurface CreatePlanarSurface(XCad.Geometry.Structures.Point center, Vector dir,
            Vector refDir)
        {
            return m_Modeler.CreatePlanarSurface2(center.ToArray(), dir.ToArray(), refDir.ToArray()) as ISurface;
        }

        private IBody2 Extrude(ISurface surf, ICurve[] boundary, MathVector dir, double height)
        {
            var sheetBody = surf.CreateTrimmedSheet4(boundary, true) as Body2;

            return m_Modeler.CreateExtrudedBody(sheetBody, dir, height) as IBody2;
        }
    }
}
