using Xarial.XCad.Base;

namespace Xarial.XCad.Geometry.Wires
{
    public interface IXSegment : IXTransaction
    {
        /// <summary>
        /// Start point of this sketch segment
        /// </summary>
        IXPoint StartPoint { get; }

        /// <summary>
        /// End point of this sketch segment
        /// </summary>
        IXPoint EndPoint { get; }
    }
}
