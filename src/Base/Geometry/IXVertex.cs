//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Represents the vertex
    /// </summary>
    public interface IXVertex : IXEntity
    {
        /// <summary>
        /// Coordinate of the vertex
        /// </summary>
        Point Coordinate { get; }
    }
}
