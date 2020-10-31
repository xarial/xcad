using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry.Primitives
{
    public interface IXRevolve : IXPrimitive
    {
        IXSegment Profile { get; set; }
        IXLine Axis { get; set; }
        double Angle { get; set; }
    }
}
