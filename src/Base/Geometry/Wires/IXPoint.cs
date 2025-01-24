//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry.Wires
{
    /// <summary>
    /// Represents the point entity
    /// </summary>
    public interface IXPoint : IXWireEntity
    {
        /// <summary>
        /// Coodinate of the point
        /// </summary>
        Point Coordinate { get; set; }
    }
}
