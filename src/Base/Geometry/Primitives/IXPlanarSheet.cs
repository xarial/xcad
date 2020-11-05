using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry.Primitives
{
    public interface IXPlanarSheet : IXPrimitive
    {
        IXSegment[] Boundary { get; set; }
    }
}
