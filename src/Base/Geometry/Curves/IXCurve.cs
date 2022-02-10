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
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry.Curves
{
    /// <summary>
    /// Represents the curve
    /// </summary>
    public interface IXCurve : IXSegment
    {
        /// <summary>
        /// Find closes point on this curve
        /// </summary>
        /// <param name="point">Input point</param>
        /// <returns></returns>
        Point FindClosestPoint(Point point);

        /// <summary>
        /// Finds u-parameter of the curve based on the point location
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>U-parameter</returns>
        double CalculateUParameter(Point point);

        /// <summary>
        /// Finds location of the point based on the curve u-parameter
        /// </summary>
        /// <param name="uParam">U-parameter</param>
        /// <returns>Point location</returns>
        Point CalculateLocation(double uParam);

        /// <summary>
        /// Calculates the length of the curve
        /// </summary>
        /// <param name="startParamU">Start U-parameter</param>
        /// <param name="endParamU">End U-parameter</param>
        /// <returns></returns>
        double CalculateLength(double startParamU, double endParamU);

        /// <summary>
        /// Creates wire body from this curve
        /// </summary>
        /// <returns>Wire body</returns>
        IXWireBody CreateBody();
    }
}
