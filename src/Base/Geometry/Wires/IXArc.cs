using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry.Wires
{
    public interface IXArc : IXSegment
    {
        double Diameter { get; set; }
        Point Center { get; set; }
    }
}
