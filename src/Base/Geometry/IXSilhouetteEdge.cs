//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
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
    public interface IXSilhouetteEdge : IXEntity, IXSegment
    {
        /// <summary>
        /// Owner face of this silhouette edge
        /// </summary>
        IXFace Face { get; }

        /// <summary>
        /// Underlyining curve defining this edge
        /// </summary>
        IXCurve Definition { get; }
    }
}