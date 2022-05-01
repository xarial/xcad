//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
    /// Represents the surface
    /// </summary>
    public interface IXSurface
    {
        /// <summary>
        /// Finds the closest point in this surface
        /// </summary>
        /// <param name="point">Input point</param>
        /// <returns>Closest point</returns>
        Point FindClosestPoint(Point point);

        /// <summary>
        /// Projects the specified point onto the surface
        /// </summary>
        /// <param name="point">Input point</param>
        /// <param name="direction">Projection direction</param>
        /// <param name="projectedPoint">Projected point or null</param>
        /// <returns>True if projected point is found, false - if not</returns>
        bool TryProjectPoint(Point point, Vector direction, out Point projectedPoint);

        /// <summary>
        /// Finds location of the point based on the u and v parameters
        /// </summary>
        /// <param name="uParam">U-parameter</param>
        /// <param name="vParam">V-parameter</param>
        /// <param name="normal">Normal vector at point</param>
        /// <returns>Point location</returns>
        Point CalculateLocation(double uParam, double vParam, out Vector normal);
    }
}
