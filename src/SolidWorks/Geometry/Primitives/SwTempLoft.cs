//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Surfaces;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;
using Xarial.XCad.SolidWorks.Geometry.Surfaces;

namespace Xarial.XCad.SolidWorks.Geometry.Primitives
{
    public interface ISwTempLoft : IXLoft, ISwTempPrimitive
    {
        new ISwPlanarRegion[] Profiles { get; set; }
    }

    public interface ISwTempSurfaceLoft : ISwTempLoft, ISwTempPrimitive
    {
    }

    internal class SwTempSurfaceLoft : SwTempPrimitive, ISwTempSurfaceLoft
    {
        IXPlanarRegion[] IXLoft.Profiles
        {
            get => Profiles;
            set => Profiles = value.Cast<ISwPlanarRegion>().ToArray();
        }

        internal SwTempSurfaceLoft(SwTempBody[] bodies, ISwApplication app, bool isCreated)
            : base(bodies, app, isCreated)
        {
        }

        public ISwPlanarRegion[] Profiles
        {
            get => m_Creator.CachedProperties.Get<ISwPlanarRegion[]>();
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

        protected override ISwTempBody[] CreateBodies(CancellationToken cancellationToken)
        {
            var bodies = new List<SwTempBody>();

            if (Profiles.Length > 1)
            {
                var profiles = Profiles.Select(p => GetSingleCurve(p.OuterLoop.IterateCurves().SelectMany(c => c.Curves).ToArray()).MakeBsplineCurve2()).ToArray();

                var guides = new ICurve[] { };
                var surf = (ISurface)m_Modeler.CreateLoftSurface(profiles, false, false, guides, 0, 0,
                    null, null, guides, guides, false, false, null, null, -1, -1, -1, -1);

                var surfParams = surf.Parameterization2();
                var uvRange = new double[] { surfParams.UMin, surfParams.UMax, surfParams.VMin, surfParams.VMax };

                var body = (IBody2)m_Modeler.CreateSheetFromSurface(surf, uvRange);

                if (body != null)
                {
                    return new ISwTempBody[] { m_App.CreateObjectFromDispatch<SwTempBody>(body, null) };
                }
                else
                {
                    throw new Exception("Failed to create loft body");
                }
            }
            else 
            {
                throw new Exception("More than one profile is required");
            }
        }
    }
}
