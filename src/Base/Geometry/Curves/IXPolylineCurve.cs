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

namespace Xarial.XCad.Geometry.Curves
{
    /// <summary>
    /// Type of the polyline using in <see cref="IXPolylineCurve"/>
    /// </summary>
    public enum PolylineMode_e 
    {
        /// <summary>
        /// Each pair of points represents an individual line coordinates
        /// </summary>
        Lines,

        /// <summary>
        /// Ent point of the previous line is the start point of the new line
        /// </summary>
        Strip,

        /// <summary>
        /// Line is created between last and first points
        /// </summary>
        Loop
    }

    /// <summary>
    /// Represents the continue curve containing lines
    /// </summary>
    public interface IXPolylineCurve : IXCurve
    {
        /// <summary>
        /// Polyline curve mode
        /// </summary>
        PolylineMode_e Mode { get; set; }

        /// <summary>
        /// End points of the polyline
        /// </summary>
        Point[] Points { get; set; }
    }
}
