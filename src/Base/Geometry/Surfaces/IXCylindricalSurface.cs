//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry.Surfaces
{
    /// <summary>
    /// Represents the specific cylindrical surface
    /// </summary>
    public interface IXCylindricalSurface : IXSurface
    {
        /// <summary>
        /// Axis of this cylindrical face
        /// </summary>
        Axis Axis { get; }

        /// <summary>
        /// Radius of cylindrical face
        /// </summary>
        double Radius { get; }
    }
}
