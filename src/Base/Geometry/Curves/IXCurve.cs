//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Xarial.XCad.Base;
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
        /// Finds the boundary of this curve
        /// </summary>
        /// <param name="uMin">Minimum u-parameter</param>
        /// <param name="uMax">Maximum u-parameter</param>
        void GetUBoundary(out double uMin, out double uMax);

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
        /// <param name="tangent">Tangent vector at point</param>
        /// <returns>Point location</returns>
        Point CalculateLocation(double uParam, out Vector tangent);

        /// <summary>
        /// Calculates the length of the curve
        /// </summary>
        /// <param name="startParamU">Start U-parameter</param>
        /// <param name="endParamU">End U-parameter</param>
        /// <returns></returns>
        double CalculateLength(double startParamU, double endParamU);

        /// <summary>
        /// Applies transform to this curve
        /// </summary>
        /// <param name="transform">Transform to apply</param>
        void Transform(TransformMatrix transform);
    }

    /// <summary>
    /// Additional methods of <see cref="IXCurve"/>
    /// </summary>
    public static class XCurveExtension 
    {
        /// <summary>
        /// Create wire body from this curve
        /// </summary>
        /// <param name="curve">Input curve</param>
        /// <returns>Wire body</returns>
        public static IXMemoryWireBody CreateBody(this IXCurve curve) 
        {
            var wireBody = curve.OwnerApplication.MemoryGeometryBuilder.WireBuilder.PreCreateWireBody();

            if (!curve.IsCommitted) 
            {
                curve.Commit();
            }

            wireBody.Segments = new IXSegment[] { curve };
            wireBody.Commit();
            return wireBody;
        }
    }
}
