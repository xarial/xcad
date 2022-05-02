using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Type of intersection
    /// </summary>
    public enum RayIntersectionType_e 
    {
        /// <summary>
        /// Ray enters the body at hit point
        /// </summary>
        Enter,

        /// <summary>
        /// Ray exists the body at hit point
        /// </summary>
        Exit
    }

    /// <summary>
    /// Information about ray intersection
    /// </summary>
    public interface IXRay : IXTransaction
    {
        /// <summary>
        /// Axis
        /// </summary>
        Axis Axis { get; }

        /// <summary>
        /// Hit result
        /// </summary>
        RayHitResult[] Hits { get; }
    }

    /// <summary>
    /// Result of ray hit
    /// </summary>
    public class RayHitResult 
    {
        /// <summary>
        /// Hit point of this ray
        /// </summary>
        public Point HitPoint { get; }

        /// <summary>
        /// Intersection face
        /// </summary>
        public IXFace Face { get; }

        /// <summary>
        /// Type of intersection
        /// </summary>
        public RayIntersectionType_e Type { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="hitPoint">Hit point</param>
        /// <param name="face">Face</param>
        /// <param name="type">Type</param>
        public RayHitResult(Point hitPoint, IXFace face, RayIntersectionType_e type) 
        {
            HitPoint = hitPoint;
            Face = face;
            Type = type;
        }
    }

    /// <summary>
    /// Performs ray intersection with the geometry
    /// </summary>
    public interface IXRayIntersection : IEvaluation
    {
        /// <summary>
        /// Rays used in the ray intersections
        /// </summary>
        IXRay[] Rays { get; }

        /// <summary>
        /// Adds new ray direction
        /// </summary>
        /// <param name="rayAxis">Axis of the ray</param>
        IXRay AddRay(Axis rayAxis);
    }

    /// <summary>
    /// Performs ray intersection with the geometry in assembly
    /// </summary>
    public interface IXAssemblyRayIntersection : IXRayIntersection, IAssemblyEvaluation
    {
    }
}
