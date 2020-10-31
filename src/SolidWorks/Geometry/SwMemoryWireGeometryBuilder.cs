using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Memory;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Geometry.Curves;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public class SwMemoryWireGeometryBuilder : IXMemoryWireGeometryBuilder
    {
        IXArc IXWireGeometryBuilder.PreCreateArc() => PreCreateArc();
        IXLine IXWireGeometryBuilder.PreCreateLine() => PreCreateLine();
        IXPoint IXWireGeometryBuilder.PreCreatePoint() => PreCreatePoint();

        public SwArc PreCreateArc() => new SwArc(m_Modeler, null, false);
        public SwLine PreCreateLine() => new SwLine(m_Modeler, null, false);
        public SwPoint PreCreatePoint() => new SwPoint();

        protected readonly IModeler m_Modeler;
        protected readonly IMathUtility m_MathUtils;

        internal SwMemoryWireGeometryBuilder(IMathUtility mathUtils, IModeler modeler)
        {
            m_MathUtils = mathUtils;
            m_Modeler = modeler;
        }
    }
}
