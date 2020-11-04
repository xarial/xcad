using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry.Primitives
{
    public interface IXRegion
    {
        Plane Plane { get; }
        IXSegment[] Boundary { get; }
    }
}
