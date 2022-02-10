//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Features;

namespace Xarial.XCad.Sketch
{
    /// <summary>
    /// Represents generic sketch entity (e.g. line, point, arc, etc.)
    /// </summary>
    public interface IXSketchEntity : IXSelObject, IXColorizable, IXTransaction
    {
        /// <summary>
        /// Owner sketch of this sketch entity
        /// </summary>
        IXSketchBase OwnerSketch { get; }
    }
}