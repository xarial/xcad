using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry.Primitives
{
    public interface IXRevolve : IXPrimitive
    {
        IXRegion[] Profiles { get; set; }
        IXLine Axis { get; set; }
        double Angle { get; set; }
    }
}
