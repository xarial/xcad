//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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
        Point Center { get; }
        Vector Axis { get; }
        double Radius { get; }
    }

    public interface IXLinearEdge : IXEdge
    {
        Point RootPoint { get; }
        Vector Direction { get; }
    }
}