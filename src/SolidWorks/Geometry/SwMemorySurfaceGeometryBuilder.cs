//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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
    }

    internal class SwMemorySheetGeometryBuilder : ISwMemorySheetGeometryBuilder
    {
        IXPlanarSheet IXSheetGeometryBuilder.PreCreatePlanarSheet() => PreCreatePlanarSheet();

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

        public ISwTempPlanarSheet PreCreatePlanarSheet() => new SwTempPlanarSheet(m_MathUtils, m_Modeler, null, false);

        protected readonly IModeler m_Modeler;
        protected readonly IMathUtility m_MathUtils;

        internal SwMemorySheetGeometryBuilder(IMathUtility mathUtils, IModeler modeler)
        {
            m_MathUtils = mathUtils;
            m_Modeler = modeler;
        }
    }
}
