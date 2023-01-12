//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry.Primitives
{
    /// <summary>
    /// Specific planar sheet
    /// </summary>
    public interface IXPlanarSheet : IXPrimitive
    {
        /// <summary>
        /// Boundary of this sheet
        /// </summary>
        IXPlanarRegion Region { get; set; }
        
        /// <inheritdoc/>
        new IXPlanarSheetBody[] Bodies { get; }
    }
}
