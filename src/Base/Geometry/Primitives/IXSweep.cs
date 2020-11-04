using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry.Primitives
{
    public interface IXSweep : IXPrimitive
    {
        IXRegion[] Profiles { get; set; }
        IXSegment Path { get; set; }
    }
}
