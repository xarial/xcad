//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry.Wires
{
    /// <summary>
    /// Represents the arc segment
    /// </summary>
    public interface IXCircle : IXSegment
    {
        /// <summary>
        /// Geometry of this circle
        /// </summary>
        Circle Geometry { get; set; }
    }

    /// <summary>
    /// Represents the arc
    /// </summary>
    public interface IXArc : IXCircle
    {
        /// <summary>
        /// Start point of the arc
        /// </summary>
        Point Start { get; set; }

        /// <summary>
        /// End point of the arc
        /// </summary>
        Point End { get; set; }
    }
}
