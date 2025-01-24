//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Annotations
{
    /// <summary>
    /// Represents the base interface of annotation (e.g.<see cref="IXDimension"/>, <see cref="IXNote"/> etc.)
    /// </summary>
    public interface IXAnnotation : IXSelObject, IHasColor, IHasLayer
    {
        /// <summary>
        /// Position of this annotation
        /// </summary>
        Point Position { get; set; }

        /// <summary>
        /// Font of this annotation
        /// </summary>
        IFont Font { get; set; }
    }

    /// <summary>
    /// Drawing specific <see cref="IXAnnotation"/>
    /// </summary>
    public interface IXDrawingAnnotation : IXAnnotation 
    {
        /// <summary>
        /// Owner of this annotation
        /// </summary>
        /// <remarks>This can be <see cref="IXDrawingView"/>, <see cref="IXSheet"/>, <see cref="IXSheetFormat"/></remarks>
        IXObject Owner { get; set; }
    }
}
