//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.SolidWorks.Geometry.Primitives;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwMemorySheetGeometryBuilder : IXSheetGeometryBuilder
    {
        new ISwTempPlanarSheet PreCreatePlanarSheet();
        new ISwTempSurfaceKnit PreCreateKnit();
    }

    internal class SwMemorySheetGeometryBuilder : ISwMemorySheetGeometryBuilder
    {
        IXPlanarSheet IXSheetGeometryBuilder.PreCreatePlanarSheet() => PreCreatePlanarSheet();
        IXKnit IX3DGeometryBuilder.PreCreateKnit() => PreCreateKnit();

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

        public ISwTempPlanarSheet PreCreatePlanarSheet() => new SwTempPlanarSheet(null, m_App, false);

        public ISwTempSurfaceKnit PreCreateKnit() => new SwTempSurfaceKnit(null, m_App, false);

        private readonly ISwApplication m_App;

        protected readonly IModeler m_Modeler;
        protected readonly IMathUtility m_MathUtils;

        internal SwMemorySheetGeometryBuilder(ISwApplication app)
        {
            m_App = app;

            m_MathUtils = m_App.Sw.IGetMathUtility();
            m_Modeler = m_App.Sw.IGetModeler();
        }
    }
}
