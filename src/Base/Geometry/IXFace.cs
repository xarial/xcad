//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Surfaces;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Represents face entity
    /// </summary>
    public interface IXFace : IXEntity, IHasColor, IXRegion
    {
        /// <summary>
        /// True if the direction of the face conicides with the direction of its surface definition, False if the directions are opposite
        /// </summary>
        bool Sense { get; }

        /// <summary>
        /// Area of the face
        /// </summary>
        double Area { get; }

        /// <summary>
        /// Underlying definition for this face
        /// </summary>
        IXSurface Definition { get; }

        /// <summary>
        /// Returns the feature which owns this face
        /// </summary>
        IXFeature Feature { get; }

        /// <summary>
        /// Projects the specified point onto the surface
        /// </summary>
        /// <param name="point">Input point</param>
        /// <param name="direction">Projection direction</param>
        /// <param name="projectedPoint">Projected point or null</param>
        /// <returns>True if projected point is found, false - if not</returns>
        bool TryProjectPoint(Point point, Vector direction, out Point projectedPoint);

        /// <summary>
        /// Finds the boundary of this face
        /// </summary>
        /// <param name="uMin">Minimum u-parameter</param>
        /// <param name="uMax">Maximum u-parameter</param>
        /// <param name="vMin">Minimum v-parameter</param>
        /// <param name="vMax">Maximum v-parameter</param>
        void GetUVBoundary(out double uMin, out double uMax, out double vMin, out double vMax);

        /// <summary>
        /// Finds u and v parameters of the face based on the point location
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="uParam">U-parameter</param>
        /// <param name="vParam">V-parameter</param>
        void CalculateUVParameter(Point point, out double uParam, out double vParam);
    }

    /// <summary>
    /// Represents planar face
    /// </summary>
    public interface IXPlanarFace : IXFace, IXPlanarRegion
    {
        /// <inheritdoc/>
        new IXPlanarSurface Definition { get; }
    }

    /// <summary>
    /// Represents cylindrical face
    /// </summary>
    public interface IXCylindricalFace : IXFace 
    {
        /// <inheritdoc/>
        new IXCylindricalSurface Definition { get; }
    }

    /// <summary>
    /// Additional methods for <see cref="IXPlanarFace"/>
    /// </summary>
    public static class XPlanarFaceExtension 
    {
        /// <summary>
        /// Returns the normal vector of the planar face
        /// </summary>
        /// <param name="face">Face to get normal from</param>
        /// <returns>Normal vector</returns>
        public static Vector GetNormal(this IXPlanarFace face)
            => face.Definition.Plane.Normal * (face.Sense ? -1 : 1);
    }

    /// <summary>
    /// Blend face
    /// </summary>
    public interface IXBlendXFace : IXFace 
    {
        /// <inheritdoc/>
        new IXBlendSurface Definition { get; }
    }

    /// <summary>
    /// B-surface face
    /// </summary>
    public interface IXBFace : IXFace
    {
        /// <inheritdoc/>
        new IXBSurface Definition { get; }
    }

    /// <summary>
    /// Conical face
    /// </summary>
    public interface IXConicalFace : IXFace
    {
        /// <inheritdoc/>
        new IXConicalSurface Definition { get; }
    }

    /// <summary>
    /// Extruded face
    /// </summary>
    public interface IXExtrudedFace : IXFace
    {
        /// <inheritdoc/>
        new IXExtrudedSurface Definition { get; }
    }

    /// <summary>
    /// Offset face
    /// </summary>
    public interface IXOffsetFace : IXFace
    {
        /// <inheritdoc/>
        new IXOffsetSurface Definition { get; }
    }

    /// <summary>
    /// Revolved face
    /// </summary>
    public interface IXRevolvedFace : IXFace
    {
        /// <inheritdoc/>
        new IXRevolvedSurface Definition { get; }
    }

    /// <summary>
    /// Spherical face
    /// </summary>
    public interface IXSphericalFace : IXFace
    {
        /// <inheritdoc/>
        new IXSphericalSurface Definition { get; }
    }

    /// <summary>
    /// Toroidal face
    /// </summary>
    public interface IXToroidalFace : IXFace
    {
        /// <inheritdoc/>
        new IXToroidalSurface Definition { get; }
    }
}
