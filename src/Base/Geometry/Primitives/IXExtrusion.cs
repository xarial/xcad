using System.Collections.Generic;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry.Primitives
{
    public interface IXExtrusion : IXPrimitive
    {
        IXSegment[] Profiles { get; set; }
        double Depth { get; set; }
        Vector Direction { get; set; }
    }
}
