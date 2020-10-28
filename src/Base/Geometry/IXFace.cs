//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Represents face entity
    /// </summary>
    public interface IXFace : IXEntity, IXColorizable
    {
        /// <summary>
        /// Area of the face
        /// </summary>
        double Area { get; }
    }

    /// <summary>
    /// Represents planar face
    /// </summary>
    public interface IXPlanarFace : IXFace 
    {
        /// <summary>
        /// Normal vector of the planar face
        /// </summary>
        Vector Normal { get; }
    }

    /// <summary>
    /// Represents cylindrical face
    /// </summary>
    public interface IXCylindricalFace : IXFace 
    {
        /// <summary>
        /// Origin of the cylindrical face
        /// </summary>
        Point Origin { get; }

        /// <summary>
        /// Cylinder axis
        /// </summary>
        Vector Axis { get; }
        
        /// <summary>
        /// Radius of cylindrical face
        /// </summary>
        double Radius { get; }
    }
}
