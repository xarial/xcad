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
        /// <returns></returns>
        IXLine PreCreateLine();

        /// <summary>
        /// Creates an arc template
        /// </summary>
        /// <returns></returns>
        IXArc PreCreateArc();

        /// <summary>
        /// Creates a point template
        /// </summary>
        /// <returns></returns>
        IXPoint PreCreatePoint();

        /// <summary>
        /// Creates a polyline template
        /// </summary>
        /// <returns></returns>
        IXPolylineCurve PreCreatePolyline();

        /// <summary>
        /// Merges the input curve into a single curve
        /// </summary>
        /// <param name="curves">Curves to merge</param>
        /// <returns>Merged curve</returns>
        IXCurve Merge(IXCurve[] curves);
    }
}
