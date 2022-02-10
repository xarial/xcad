//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Geometry.Structures
{
    /// <summary>
    /// Represents the plane
    /// </summary>
    public class Plane
    {
        /// <summary>
        /// Root point of this plane
        /// </summary>
        public Point Point { get; set; }

        /// <summary>
        /// Normal of this plane
        /// </summary>
        public Vector Normal { get; set; }

        /// <summary>
        /// Direction of this plane (X axis)
        /// </summary>
        public Vector Direction { get; set; }

        /// <summary>
        /// Reference vector of this plane (Y axis)
        /// </summary>
        public Vector Reference => Normal.Cross(Direction);

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="point">Origin point</param>
        /// <param name="normal">Plane normal</param>
        /// <param name="direction">Plane reference direction</param>
        public Plane(Point point, Vector normal, Vector direction) 
        {
            Point = point;
            Normal = normal;
            Direction = direction;
        }
    }

    /// <summary>
    /// Additional methods for <see cref="Plane"/>
    /// </summary>
    public static class PlaneExtension 
    {
        /// <summary>
        /// Gets the transformation of this plane relative to the global XYZ
        /// </summary>
        /// <param name="plane">Plane</param>
        /// <returns>Transformation matrix</returns>
        public static TransformMatrix GetTransformation(this Plane plane)
            => TransformMatrix.Compose(plane.Direction, plane.Reference, plane.Normal, plane.Point);
    }
}
