//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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
    public interface IXCylindricalSurface : IXSurface
    {
        /// <summary>
        /// Origin of the cylindrical face
        /// </summary>
        Point Origin { get; }

        /// <summary>
        /// Cylinder axis
        /// </summary>
        Vector Axis { get; }

        /// <summary>
        /// Radius of cylindrical face
        /// </summary>
        double Radius { get; }
    }
}
