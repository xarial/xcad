using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry
{
    public interface IXWireGeometryBuilder
    {
        IXLine PreCreateLine();
        IXArc PreCreateArc();
        IXPoint PreCreatePoint();
        IXPolylineCurve PreCreatePolyline();
        IXComplexCurve PreCreateComplex();
    }
}
