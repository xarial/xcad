using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Memory;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.SolidWorks.Geometry.Primitives;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public class SwMemorySurfaceGeometryBuilder : IXMemorySurfaceGeometryBuilder
    {
        IXPlanarSurface IXSurfaceGeometryBuilder.PreCreatePlanarSurface() => PreCreatePlanarSurface();

        public IXExtrusion PreCreateExtrusion()
        {
            throw new NotImplementedException();
        }

        public IXLoft PreCreateLoft()
        {
            throw new NotImplementedException();
        }

        public IXRevolve PreCreateRevolve()
        {
            throw new NotImplementedException();
        }

        public IXSweep PreCreateSweep()
        {
            throw new NotImplementedException();
        }

        public SwTempPlanarSurface PreCreatePlanarSurface() => new SwTempPlanarSurface(m_MathUtils, m_Modeler, null, false);

        protected readonly IModeler m_Modeler;
        protected readonly IMathUtility m_MathUtils;

        internal SwMemorySurfaceGeometryBuilder(IMathUtility mathUtils, IModeler modeler)
        {
            m_MathUtils = mathUtils;
            m_Modeler = modeler;
        }
    }
}
