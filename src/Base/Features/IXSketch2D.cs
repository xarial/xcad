//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Features
{
    /// <summary>
    /// Represents specific 2D sketch
    /// </summary>
    public interface IXSketch2D : IXSketchBase
    {
        /// <summary>
        /// Regions in this 2D sketch
        /// </summary>
        IEnumerable<IXSketchRegion> Regions { get; }

        /// <summary>
        /// Returns the plane of this sketch
        /// </summary>
        Plane Plane { get; }

        /// <summary>
        /// Entity where this sketch is based on
        /// </summary>
        IXPlanarRegion ReferenceEntity { get; set; }
    }
}