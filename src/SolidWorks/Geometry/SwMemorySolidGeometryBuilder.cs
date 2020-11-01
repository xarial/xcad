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
    public class SwMemorySolidGeometryBuilder : IXMemorySolidGeometryBuilder
    {
        IXExtrusion IX3DGeometryBuilder.PreCreateExtrusion() => PreCreateExtrusion();
        IXRevolve IX3DGeometryBuilder.PreCreateRevolve() => PreCreateRevolve();

        public IXLoft PreCreateLoft()
        {
            throw new NotImplementedException();
        }
        
        public IXSweep PreCreateSweep()
        {
            throw new NotImplementedException();
        }

        protected readonly IModeler m_Modeler;
        protected readonly IMathUtility m_MathUtils;

        internal SwMemorySolidGeometryBuilder(IMathUtility mathUtils, IModeler modeler)
        {
            m_MathUtils = mathUtils;
            m_Modeler = modeler;
        }

        public SwTempExtrusion PreCreateExtrusion() => new SwTempExtrusion(m_MathUtils, m_Modeler, null, false);
        public SwTempRevolve PreCreateRevolve() => new SwTempRevolve(m_MathUtils, m_Modeler, null, false);
    }
}
