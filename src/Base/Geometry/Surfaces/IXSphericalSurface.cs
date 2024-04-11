//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry.Surfaces
{
    /// <summary>
    /// Spherical surface
    /// </summary>
    public interface IXSphericalSurface : IXSurface
    {
        /// <summary>
        /// Radius of the surface
        /// </summary>
        double Radius { get; }

        /// <summary>
        /// Center point of the surface
        /// </summary>
        Point Origin { get; }
    }
}
