//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.SolidWorks.Services;

namespace Xarial.XCad.SolidWorks.Geometry.Primitives
{
    public interface ISwTempKnit : IXKnit, ISwTempPrimitive
    {
    }

    public interface ISwTempSurfaceKnit : ISwTempKnit, ISwTempPrimitive
    {
        new ISwTempSheetBody[] Bodies { get; }
    }

    public interface ISwTempSolidKnit : ISwTempKnit, ISwTempPrimitive
    {
        new ISwTempSolidBody[] Bodies { get; }
    }

    internal abstract class SwTempKnit : SwTempPrimitive, ISwTempKnit 
    {
        private readonly IMemoryGeometryBuilderToleranceProvider m_TolProvider;

        internal SwTempKnit(SwTempBody[] bodies, ISwApplication app, bool isCreated, IMemoryGeometryBuilderToleranceProvider tolProvider) : base(bodies, app, isCreated)
        {
            m_TolProvider = tolProvider;
        }

        public IXRegion[] Regions
        {
            get => m_Creator.CachedProperties.Get<IXRegion[]>();
            set
            {
                if (IsCommitted)
                {
                    throw new Exception("Cannot change faces for the commited temp knit");
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        protected override ISwTempBody[] CreateBodies(CancellationToken cancellationToken)
        {
            if (Regions?.Length > 0)
            {
                var sheetBodies = Regions.Select(f =>
                {
                    switch (f) 
                    {
                        case ISwPlanarRegion planarReg:
                            return planarReg.PlanarSheetBody.Body;

                        case ISwFace face:
                            return face.Face.ICreateSheetBody();

                        default:
                            throw new Exception("Not supported region for the knit: specify planar region or face");
                    }
                    
                }).ToArray();
                
                int errs = -1;

                var bodies = (object[])m_Modeler.CreateBodiesFromSheets2(sheetBodies, (int)SewingType, m_TolProvider.Knitting, ref errs);

                if (bodies?.Any() == true)
                {
                    var swBodies = bodies.Cast<IBody2>().ToArray();

                    ValidateBodies(swBodies);

                    return swBodies.Select(b => m_App.CreateObjectFromDispatch<ISwTempBody>(b, null)).ToArray();
                }
                else
                {
                    throw new Exception($"Failed to knit surfaces into a body. Error code: {(swSheetSewingError_e)errs} ({errs})");
                }
            }
            else
            {
                throw new Exception("No faces specified for knitting");
            }
        }

        protected abstract void ValidateBodies(IBody2[] bodies);

        protected abstract swSheetSewingOption_e SewingType { get; }
    }

    internal class SwTempSurfaceKnit : SwTempKnit, ISwTempSurfaceKnit
    {
        ISwTempSheetBody[] ISwTempSurfaceKnit.Bodies => (ISwTempSheetBody[])base.Bodies;

        protected override swSheetSewingOption_e SewingType => swSheetSewingOption_e.swSewToSheets;

        internal SwTempSurfaceKnit(SwTempBody[] bodies, ISwApplication app, bool isCreated, IMemoryGeometryBuilderToleranceProvider tolProvider) : base(bodies, app, isCreated, tolProvider)
        {
        }

        protected override void ValidateBodies(IBody2[] bodies)
        {
            if (bodies.Any(b => b.GetType() != (int)swBodyType_e.swSheetBody)) 
            {
                throw new Exception("Result of the knit operation contains non-sheet bodies");
            }
        }
    }

    internal class SwTempSolidKnit : SwTempKnit, ISwTempSolidKnit
    {
        ISwTempSolidBody[] ISwTempSolidKnit.Bodies => (ISwTempSolidBody[])base.Bodies;

        protected override swSheetSewingOption_e SewingType => swSheetSewingOption_e.swSewToSolid;

        internal SwTempSolidKnit(SwTempBody[] bodies, ISwApplication app, bool isCreated, IMemoryGeometryBuilderToleranceProvider tolProvider) : base(bodies, app, isCreated, tolProvider)
        {
        }

        protected override void ValidateBodies(IBody2[] bodies)
        {
            if (bodies.Any(b => b.GetType() != (int)swBodyType_e.swSolidBody))
            {
                throw new Exception("Result of the knit operation contains non-solid bodies");
            }
        }
    }
}
