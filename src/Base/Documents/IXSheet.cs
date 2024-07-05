//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Features;
using Xarial.XCad.UI;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Sheet format sketch
    /// </summary>
    public interface IXSheetFormat : IXObject
    {
        /// <summary>
        /// Template of this sheet format
        /// </summary>
        string Template { get; set; }

        /// <summary>
        /// Sketch of this sheet format
        /// </summary>
        IXSketch2D Sketch { get; }
    }

    /// <summary>
    /// Represents the drawing sheet
    /// </summary>
    public interface IXSheet : IXSelObject, IXTransaction
    {
        /// <summary>
        /// Name of the sheet
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Drawings views on this sheet
        /// </summary>
        IXDrawingViewRepository DrawingViews { get; }

        /// <summary>
        /// Collection of annotations
        /// </summary>
        IXAnnotationRepository Annotations { get; }

        /// <summary>
        /// Sketch space of this sheet
        /// </summary>
        IXSketch2D Sketch { get; }

        /// <summary>
        /// Sketch space of this sheet format
        /// </summary>
        IXSheetFormat Format { get; }

        /// <summary>
        /// Preview of this drawing sheet
        /// </summary>
        IXImage Preview { get; }

        /// <summary>
        /// Represents scale of this sheet
        /// </summary>
        Scale Scale { get; set; }

        /// <summary>
        /// Represents paper of this sheet
        /// </summary>
        PaperSize PaperSize { get; set; }

        /// <summary>
        /// Types of the projection of drawing views in this sheet
        /// </summary>
        ViewsProjectionType_e ViewsProjectionType { get; set; }

        /// <summary>
        /// Creates a copy of this sheet
        /// </summary>
        /// <param name="targetDrawing">Drawing where to copy sheet to</param>
        /// <returns>Cloned sheet</returns>
        IXSheet Clone(IXDrawing targetDrawing);
    }
}
