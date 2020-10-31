using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry.Wires
{
    public interface IXArc : IXSegment
    {
        //TODO: add support for the arc with start and end point

        double Diameter { get; set; }
        Point Center { get; set; }
        Vector Axis { get; set; }
    }
}
