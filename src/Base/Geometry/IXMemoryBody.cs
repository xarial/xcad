//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Represents the memory (temp) body object
    /// </summary>
    public interface IXMemoryBody : IXBody, IDisposable
    {
        /// <summary>
        /// DIsplays the preview of the memory body
        /// </summary>
        /// <param name="context">Context where preview should be displayed (e.g. document or component)</param>
        /// <param name="color">Color of the body</param>
        /// <param name="selectable">True to allow users selecting body entities, False to disallow selection</param>
        void Preview(IXObject context, Color color, bool selectable);

        /// <summary>
        /// Boolean add operation on body
        /// </summary>
        /// <param name="other">Other body</param>
        /// <returns>Resulting body</returns>
        /// <exception cref="Exceptions.BodyBooleanOperationNoIntersectException"/>
        IXMemoryBody Add(IXMemoryBody other);

        /// <summary>
        /// Boolean subtract operation
        /// </summary>
        /// <param name="other">Body to subtract</param>
        /// <returns>Resulting bodies</returns>
        /// <exception cref="Exceptions.BodyBooleanOperationNoIntersectException"/>
        IXMemoryBody[] Subtract(IXMemoryBody other);

        /// <summary>
        /// Boolean common operation
        /// </summary>
        /// <param name="other">Body to get common with</param>
        /// <returns>Resulting body</returns>
        /// <exception cref="Exceptions.BodyBooleanOperationNoIntersectException"/>
        IXMemoryBody[] Common(IXMemoryBody other);

        /// <summary>
        /// Moves this body with specified matrix
        /// </summary>
        /// <param name="transform">Transformation matrix</param>
        void Transform(TransformMatrix transform);
    }

    /// <summary>
    /// Represents the memory (temp) sheet (surface) body
    /// </summary>
    public interface IXMemorySheetBody : IXMemoryBody, IXSheetBody
    {
    }

    /// <summary>
    /// Subtype of <see cref="IXMemorySheetBody"/> which is planar
    /// </summary>
    public interface IXMemoryPlanarSheetBody : IXMemorySheetBody, IXPlanarSheetBody
    {
    }

    /// <summary>
    /// Represents the memory (temp) solid body geometry
    /// </summary>
    public interface IXMemorySolidBody : IXMemoryBody, IXSolidBody
    {
    }

    /// <summary>
    /// Represents the the memory (temp) wire body
    /// </summary>
    public interface IXMemoryWireBody : IXMemoryBody, IXWireBody
    {
    }
}