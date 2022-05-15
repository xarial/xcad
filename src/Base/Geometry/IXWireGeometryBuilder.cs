//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
    public interface IXWireGeometryBuilder : IXRepository<IXWireEntity>
    {
        /// <summary>
        /// Merges the input curve into a single curve
        /// </summary>
        /// <param name="curves">Curves to merge</param>
        /// <returns>Merged curve</returns>
        IXCurve Merge(IXCurve[] curves);
    }

    /// <summary>
    /// Additional methods for <see cref="IXWireGeometryBuilder"/>
    /// </summary>
    public static class XWireGeometryBuilderExtension 
    {
        /// <summary>
        /// Creates a line template
        /// </summary>
        /// <returns>Line template</returns>
        public static IXLine PreCreateLine(this IXWireGeometryBuilder geomBuilder) => geomBuilder.PreCreate<IXLine>();

        /// <summary>
        /// Creates an circle template
        /// </summary>
        /// <returns>Circle template</returns>
        public static IXCircle PreCreateCircle(this IXWireGeometryBuilder geomBuilder) => geomBuilder.PreCreate<IXCircle>();

        /// <summary>
        /// Creates an arc template
        /// </summary>
        /// <returns>Arc template</returns>
        public static IXArc PreCreateArc(this IXWireGeometryBuilder geomBuilder) => geomBuilder.PreCreate<IXArc>();

        /// <summary>
        /// Creates a point template
        /// </summary>
        /// <returns>Arc template</returns>
        public static IXPoint PreCreatePoint(this IXWireGeometryBuilder geomBuilder) => geomBuilder.PreCreate<IXPoint>();

        /// <summary>
        /// Creates a polyline template
        /// </summary>
        /// <returns>Polyline template</returns>
        public static IXPolylineCurve PreCreatePolyline(this IXWireGeometryBuilder geomBuilder) => geomBuilder.PreCreate<IXPolylineCurve>();
    }
}
