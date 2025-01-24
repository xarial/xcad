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

namespace Xarial.XCad.Geometry.Surfaces
{
    /// <summary>
    /// Toroidal surface
    /// </summary>
    public interface IXToroidalSurface : IXSurface
    {
        /// <summary>
        /// Axis of toroidal surface
        /// </summary>
        Axis Axis { get; }

        /// <summary>
        /// Major radius
        /// </summary>
        double MajorRadius { get; }
        
        /// <summary>
        /// Minor radius
        /// </summary>
        double MinorRadius { get; }
    }
}
