//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Represents an edge element of the geometry
    /// </summary>
    public interface IXEdge : IXEntity, IXSegment
    {
        /// <summary>
        /// Start vertex
        /// </summary>
        new IXVertex StartPoint { get; }

        /// <summary>
        /// End vertex
        /// </summary>
        new IXVertex EndPoint { get; }

        /// <summary>
        /// Underlyining curve defining this edge
        /// </summary>
        IXCurve Definition { get; }
    }

    /// <summary>
    /// Represents specific circular edge
    /// </summary>
    public interface IXCircularEdge : IXEdge 
    {
        /// <inheritdoc/>
        new IXCircle Definition { get; }
    }

    /// <summary>
    /// Represents specific linear edge
    /// </summary>
    public interface IXLinearEdge : IXEdge
    {
        /// <inheritdoc/>
        new IXLine Definition { get; }
    }
}