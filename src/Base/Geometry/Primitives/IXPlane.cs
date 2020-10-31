using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry.Primitives
{
    public interface IXPlane : IXPrimitive
    {
        Point Center { get; set; }
        Vector Normal { get; set; }
        Vector Axis { get; set; }
        Vector Reference { get; set; }
    }
}
