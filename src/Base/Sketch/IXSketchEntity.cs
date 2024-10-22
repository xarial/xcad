//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Sketch
{
    /// <summary>
    /// Represents generic sketch entity (e.g. line, point, arc, etc.)
    /// </summary>
    public interface IXSketchEntity : IXSelObject, IHasColor, IXTransaction, IHasName, IXWireEntity, IHasLayer
    {
        /// <summary>
        /// Id of this sketch entity
        /// </summary>
        IXIdentifier Id { get; }

        /// <summary>
        /// Owner sketch of this sketch entity
        /// </summary>
        IXSketchBase OwnerSketch { get; }

        /// <summary>
        /// Gets the block where this enityt belongs to or null if not a part of the block
        /// </summary>
        IXSketchBlockInstance OwnerBlock { get; }
    }
}