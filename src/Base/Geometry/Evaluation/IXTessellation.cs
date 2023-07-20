//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry.Evaluation
{
    /// <summary>
    /// Triangle representing a tesselation
    /// </summary>
    public class TesselationTriangle
    {
        /// <summary>
        /// Normal of the triangle
        /// </summary>
        public Vector Normal { get; }

        /// <summary>
        /// First point of the triangle
        /// </summary>
        public Point FirstPoint { get; }

        /// <summary>
        /// Second point of the triangle
        /// </summary>
        public Point SecondPoint { get; }

        /// <summary>
        /// Third point of the triangle
        /// </summary>
        public Point ThirdPoint { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TesselationTriangle(Vector normal, Point firstPoint, Point secondPoint, Point thirdPoint)
        {
            Normal = normal;
            FirstPoint = firstPoint;
            SecondPoint = secondPoint;
            ThirdPoint = thirdPoint;
        }
    }

    /// <summary>
    /// Provides the tesselation data for the geometry
    /// </summary>
    public interface IXTessellation : IEvaluation
    {
        /// <summary>
        /// Triangulation of the geometry
        /// </summary>
        IEnumerable<TesselationTriangle> Triangles { get; }
    }

    /// <summary>
    /// Tesselation specific to the assembly
    /// </summary>
    public interface IXAssemblyTessellation : IXTessellation, IAssemblyEvaluation
    {
    }
}
