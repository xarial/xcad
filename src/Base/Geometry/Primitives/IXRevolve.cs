using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry.Primitives
{
    public interface IXRevolve : IXPrimitive
    {
        IXRegion Profile { get; set; }
        IXLine Axis { get; set; }
        double Angle { get; set; }
    }
}
