//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using Xarial.XCad.Geometry.Curves;
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
        IXPlanarRegion[] Profiles { get; set; }

        /// <summary>
        /// Depth of the extrusion
        /// </summary>
        double Depth { get; set; }

        /// <summary>
        /// Direction of the extrusion
        /// </summary>
        Vector Direction { get; set; }
    }

    /// <summary>
    /// Represents the extruded geometry of the sheet
    /// </summary>
    public interface IXSheetExtrusion : IXExtrusion 
    {
        /// <summary>
        /// Profiles of the extrusion
        /// </summary>
        new IXCurve[] Profiles { get; set; }
    }
}
