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

        public ISwArcCurve PreCreateArc() => new SwArcCurve(m_Modeler, null, false);
        public ISwLineCurve PreCreateLine() => new SwLineCurve(m_Modeler, null, false);
        public ISwPoint PreCreatePoint() => new SwPoint();
        public IXPolylineCurve PreCreatePolyline() => new SwPolylineCurve(m_Modeler, null, false);
        public IXComplexCurve PreCreateComplex() => new SwComplexCurve(m_Modeler, null, false);

        protected readonly IModeler m_Modeler;
        protected readonly IMathUtility m_MathUtils;

        internal SwMemoryWireGeometryBuilder(IMathUtility mathUtils, IModeler modeler)
        {
            m_MathUtils = mathUtils;
            m_Modeler = modeler;
        }
    }
}
