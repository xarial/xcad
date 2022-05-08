//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry.Primitives
{
    /// <summary>
    /// Represents the extruded geometry
    /// </summary>
    public interface IXExtrusion : IXPrimitive
    {
        /// <summary>
        /// Profiles used to create this extrusion geometry
        /// </summary>
        IXRegion[] Profiles { get; set; }

        /// <summary>
        /// Depth of the extrusion
        /// </summary>
        double Depth { get; set; }

        /// <summary>
        /// Direction of the extrusion
        /// </summary>
        Vector Direction { get; set; }
    }
}
