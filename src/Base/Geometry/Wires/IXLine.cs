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
    /// Represents a line segment
    /// </summary>
    public interface IXLine : IXSegment
    {
        /// <summary>
        /// Geometry of this line
        /// </summary>
        Line Geometry { get; set; }
    }
}
