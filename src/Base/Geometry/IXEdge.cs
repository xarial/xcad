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
        /// Underlyining segment defining this edge
        /// </summary>
        IXSegment Definition { get; }
    }

    public interface IXCircularEdge : IXEdge 
    {
        new IXArc Definition { get; }
    }

    public interface IXLinearEdge : IXEdge
    {
        new IXLine Definition { get; }
    }
}