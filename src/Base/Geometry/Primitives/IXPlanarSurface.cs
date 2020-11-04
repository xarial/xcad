using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry.Primitives
{
    public interface IXPlanarSurface : IXPrimitive, IXRegion
    {
        IXSegment[] Boundary { get; set; }
    }
}
