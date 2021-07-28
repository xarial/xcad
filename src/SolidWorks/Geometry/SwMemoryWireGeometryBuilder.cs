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
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Geometry.Curves;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwMemoryWireGeometryBuilder : IXWireGeometryBuilder
    {
        new ISwLineCurve PreCreateLine();
    }

    internal class SwMemoryWireGeometryBuilder : ISwMemoryWireGeometryBuilder
    {
        IXArc IXWireGeometryBuilder.PreCreateArc() => PreCreateArc();
        IXLine IXWireGeometryBuilder.PreCreateLine() => PreCreateLine();
        IXPoint IXWireGeometryBuilder.PreCreatePoint() => PreCreatePoint();
        IXPolylineCurve IXWireGeometryBuilder.PreCreatePolyline() => PreCreatePolyline();
        IXComplexCurve IXWireGeometryBuilder.PreCreateComplex() => PreCreateComplex();

        public ISwArcCurve PreCreateArc() => new SwArcCurve(null, null, m_App, false);
        public ISwLineCurve PreCreateLine() => new SwLineCurve(null, null, m_App, false);
        public ISwPoint PreCreatePoint() => new SwPoint();
        public IXPolylineCurve PreCreatePolyline() => new SwPolylineCurve(null, null, m_App, false);
        public IXComplexCurve PreCreateComplex() => new SwComplexCurve(null, null, m_App, false);

        private readonly ISwApplication m_App;
        protected readonly IModeler m_Modeler;
        protected readonly IMathUtility m_MathUtils;

        internal SwMemoryWireGeometryBuilder(ISwApplication app)
        {
            m_App = app;
            m_MathUtils = app.Sw.IGetMathUtility();
            m_Modeler = app.Sw.IGetModeler();
        }
    }
}
