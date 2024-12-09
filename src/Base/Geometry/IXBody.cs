//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Represents the body object
    /// </summary>
    public interface IXBody : IXSelObject, IHasColor, IXTransaction
    {
        /// <summary>
        /// Name of the body
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Is body visible
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// parent component of this body if within assembly
        /// </summary>
        /// <remarks>Null is returned for the body in the part</remarks>
        IXComponent Component { get; }

        /// <summary>
        /// Enumerates all faces of this body
        /// </summary>
        IEnumerable<IXFace> Faces { get; }

        /// <summary>
        /// Enumerates all edges of this body
        /// </summary>
        IEnumerable<IXEdge> Edges { get; }

        /// <summary>
        /// Material of this body
        /// </summary>
        IXMaterial Material { get; set; }

        /// <summary>
        /// Creates a copy of the current body
        /// </summary>
        /// <returns>Copied body</returns>
        IXMemoryBody Copy();
    }

    /// <summary>
    /// Represents sheet (surface) body
    /// </summary>
    public interface IXSheetBody : IXBody
    {
    }

    /// <summary>
    /// Subtype of <see cref="IXSheetBody"/> which is planar
    /// </summary>
    public interface IXPlanarSheetBody : IXSheetBody, IXPlanarRegion 
    {
    }

    /// <summary>
    /// Represents solid body geometry
    /// </summary>
    public interface IXSolidBody : IXBody 
    {
        /// <summary>
        /// Volume of this solid body
        /// </summary>
        double Volume { get; }
    }

    /// <summary>
    /// Represents sheet metal body
    /// </summary>
    public interface IXSheetMetalBody : IXSolidBody 
    {
    }

    /// <summary>
    /// Represents the wire body
    /// </summary>
    public interface IXWireBody : IXBody, IXWireEntity
    {
        /// <summary>
        /// Content of the wire body
        /// </summary>
        IXSegment[] Segments { get; set; }
    }
}