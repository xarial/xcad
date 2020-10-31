using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry.Primitives
{
    public interface IXSweep : IXPrimitive
    {
        IXSegment Profile { get; set; }
        IXSegment Path { get; set; }
    }
}
