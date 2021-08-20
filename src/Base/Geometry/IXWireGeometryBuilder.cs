//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Builds wire (1-dimensional) geometry
    /// </summary>
    public interface IXWireGeometryBuilder
    {
        /// <summary>
        /// Creates a line template
        /// </summary>
        /// <returns>Line template</returns>
        IXLine PreCreateLine();

        /// <summary>
        /// Creates an circle template
        /// </summary>
        /// <returns>Circle template</returns>
        IXCircle PreCreateCircle();

        /// <summary>
        /// Creates an arc template
        /// </summary>
        /// <returns>Arc template</returns>
        IXArc PreCreateArc();

        /// <summary>
        /// Creates a point template
        /// </summary>
        /// <returns>Point template</returns>
        IXPoint PreCreatePoint();

        /// <summary>
        /// Creates a polyline template
        /// </summary>
        /// <returns>Polyline template</returns>
        IXPolylineCurve PreCreatePolyline();

        /// <summary>
        /// Merges the input curve into a single curve
        /// </summary>
        /// <param name="curves">Curves to merge</param>
        /// <returns>Merged curve</returns>
        IXCurve Merge(IXCurve[] curves);
    }
}
