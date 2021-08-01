//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Represents an edge element of the geometry
    /// </summary>
    public interface IXEdge : IXEntity
    {
        /// <summary>
        /// Start vertex
        /// </summary>
        IXVertex StartVertex { get; }

        /// <summary>
        /// End vertex
        /// </summary>
        IXVertex EndVertex { get; }

        /// <summary>
        /// Underlyining segment defining this edge
        /// </summary>
        IXSegment Definition { get; }
    }

    /// <summary>
    /// Represents specific circular edge
    /// </summary>
    public interface IXCircularEdge : IXEdge 
    {
        /// <inheritdoc/>
        new IXArc Definition { get; }
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