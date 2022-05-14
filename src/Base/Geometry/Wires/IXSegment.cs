//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;

namespace Xarial.XCad.Geometry.Wires
{
    /// <summary>
    /// Segment represents the definition of any wire body
    /// </summary>
    /// <remarks>This is a based interface for all geometrical primitives (either curves, sketch segments or edges)</remarks>
    public interface IXSegment : IXTransaction, IXObject
    {
        /// <summary>
        /// Start point of this sketch segment
        /// </summary>
        IXPoint StartPoint { get; }

        /// <summary>
        /// End point of this sketch segment
        /// </summary>
        IXPoint EndPoint { get; }

        /// <summary>
        /// Length of the segment
        /// </summary>
        double Length { get; }
    }
}
