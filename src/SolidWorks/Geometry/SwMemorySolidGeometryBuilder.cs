using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Memory;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Primitives;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public class SwMemorySolidGeometryBuilder : IXMemorySolidGeometryBuilder
    {
        IXExtrusion IX3DGeometryBuilder.PreCreateExtrusion() => PreCreateExtrusion();
        IXRevolve IX3DGeometryBuilder.PreCreateRevolve() => PreCreateRevolve();
        IXSweep IX3DGeometryBuilder.PreCreateSweep() => PreCreateSweep();

        public IXLoft PreCreateLoft()
        {
            throw new NotImplementedException();
        }

        private readonly SwApplication m_App;

        protected readonly IModeler m_Modeler;
        protected readonly IMathUtility m_MathUtils;

        internal SwMemorySolidGeometryBuilder(SwApplication app)
        {
            m_App = app;

            m_MathUtils = m_App.Sw.IGetMathUtility();
            m_Modeler = m_App.Sw.IGetModeler();
        }

        public SwTempExtrusion PreCreateExtrusion() => new SwTempExtrusion(m_MathUtils, m_Modeler, null, false);
        public SwTempRevolve PreCreateRevolve() => new SwTempRevolve(m_MathUtils, m_Modeler, null, false);
        public SwTempSweep PreCreateSweep() => new SwTempSweep((SwPart)m_App.Documents.Active, m_MathUtils, m_Modeler, null, false);
    }
}
