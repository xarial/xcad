using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Geometry
{
    public interface IXGeometryBuilder
    {
        IXWireGeometryBuilder WireBuilder { get; }
        IXSurfaceGeometryBuilder SurfaceBuilder { get; }
        IXSolidGeometryBuilder SolidBuilder { get; }
    }
}
