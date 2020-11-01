using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry.Primitives
{
    public interface IXLoft : IXPrimitive
    {
        IXSegment[] Profiles { get; set; }
    }
}
