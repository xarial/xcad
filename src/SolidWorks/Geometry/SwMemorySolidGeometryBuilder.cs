//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Primitives;
using Xarial.XCad.SolidWorks.Services;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwMemorySolidGeometryBuilder : IXSolidGeometryBuilder
    {
        new ISwTempExtrusion PreCreateExtrusion();
        new ISwTempRevolve PreCreateRevolve();
        new ISwTempSweep PreCreateSweep();
        new ISwTempSolidKnit PreCreateKnit();
    }

    internal class SwMemorySolidGeometryBuilder : ISwMemorySolidGeometryBuilder
    {
        IXExtrusion IX3DGeometryBuilder.PreCreateExtrusion() => PreCreateExtrusion();
        IXRevolve IX3DGeometryBuilder.PreCreateRevolve() => PreCreateRevolve();
        IXSweep IX3DGeometryBuilder.PreCreateSweep() => PreCreateSweep();
        IXKnit IX3DGeometryBuilder.PreCreateKnit() => PreCreateKnit();

        public IXLoft PreCreateLoft()
        {
            throw new NotImplementedException();
        }

        private readonly ISwApplication m_App;

        protected readonly IModeler m_Modeler;
        protected readonly IMathUtility m_MathUtils;

        private readonly IMemoryGeometryBuilderDocumentProvider m_GeomBuilderDocsProvider;

        internal SwMemorySolidGeometryBuilder(ISwApplication app, IMemoryGeometryBuilderDocumentProvider geomBuilderDocsProvider)
        {
            m_App = app;

            m_MathUtils = m_App.Sw.IGetMathUtility();
            m_Modeler = m_App.Sw.IGetModeler();

            m_GeomBuilderDocsProvider = geomBuilderDocsProvider;
        }

        public ISwTempExtrusion PreCreateExtrusion() => new SwTempExtrusion(null, m_App, false);
        public ISwTempRevolve PreCreateRevolve() => new SwTempRevolve(null, m_App, false);
        public ISwTempSweep PreCreateSweep() => new SwTempSweep(null, (SwPart)m_GeomBuilderDocsProvider.ProvideDocument(typeof(SwTempSweep)), m_App, false);
        public ISwTempSolidKnit PreCreateKnit() => new SwTempSolidKnit(null, m_App, false);
    }
}
