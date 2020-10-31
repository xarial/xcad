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
    public class SwTempExtrusion : SwTempPrimitive, IXExtrusion
    {
        IXSegment[] IXExtrusion.Profiles
        {
            get => Profiles;
            set => Profiles = value.Cast<SwPlanarCurve>().ToArray();
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

        public SwPlanarCurve[] Profiles
        {
            get => m_Creator.CachedProperties.Get<SwPlanarCurve[]>();
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
            var surf = CreatePlanarSurface(Profiles.First().Plane.Point,
                Profiles.First().Plane.Normal, Profiles.First().Plane.Direction);

            var dir = m_MathUtils.CreateVector(Direction.ToArray()) as MathVector;

            var boundary = new List<ICurve>();

            for (int i = 0; i < Profiles.Length; i++) 
            {
                boundary.AddRange(Profiles[i].Curves);

                if (i != Profiles.Length - 1) 
                {
                    boundary.Add(null);
                }
            }

            var body = Extrude(surf, boundary.ToArray(), dir, Depth);

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
