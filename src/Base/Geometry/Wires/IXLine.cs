using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry.Wires
{
    public interface IXLine : IXSegment
    {
        Point StartCoordinate { get; set; }
        Point EndCoordinate { get; set; }
    }
}
