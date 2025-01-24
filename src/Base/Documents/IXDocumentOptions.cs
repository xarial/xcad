//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Visibility of the entity kinds
    /// </summary>
    public interface IXViewEntityKindVisibilityOptions
    {
        /// <summary>
        /// Axes
        /// </summary>
        bool Axes { get; set; }

        /// <summary>
        /// Temporary Axes
        /// </summary>
        bool TemporaryAxes { get; set; }

        /// <summary>
        /// Coordinate Systems
        /// </summary>
        bool CoordinateSystems { get; set; }

        /// <summary>
        /// Curves
        /// </summary>
        bool Curves { get; set; }

        /// <summary>
        /// Dimension Names
        /// </summary>
        bool DimensionNames { get; set; }

        /// <summary>
        /// Origins
        /// </summary>
        bool Origins { get; set; }

        /// <summary>
        /// Planes
        /// </summary>
        bool Planes { get; set; }

        /// <summary>
        /// Points
        /// </summary>
        bool Points { get; set; }

        /// <summary>
        /// Sketches
        /// </summary>
        bool Sketches { get; set; }

        /// <summary>
        /// Sheet Metal Bend Lines
        /// </summary>
        bool BendLines { get; set; }

        /// <summary>
        /// Sketch Dimensions
        /// </summary>
        bool SketchDimensions { get; set; }

        /// <summary>
        /// Sketch Planes
        /// </summary>
        bool SketchPlanes { get; set; }

        /// <summary>
        /// Sketch Relations
        /// </summary>
        bool SketchRelations { get; set; }

        /// <summary>
        /// Hides all entitites
        /// </summary>
        void HideAll();

        /// <summary>
        /// Shows all entities
        /// </summary>
        void ShowAll();
    }

    /// <summary>
    /// Document specific options
    /// </summary>
    public interface IXDocumentOptions : IXOptions
    {
        /// <summary>
        /// Visibility of the specific entity kinds
        /// </summary>
        IXViewEntityKindVisibilityOptions ViewEntityKindVisibility { get; }

        /// <summary>
        /// Annotations drafting standard
        /// </summary>
        IXAnnotationsDraftingStandardOptions AnnotationsDraftingStandard { get; }

        /// <summary>
        /// Dimensions drafting standard
        /// </summary>
        IXDimensionsDraftingStandardOptions DimensionsDraftingStandard { get; }

        /// <summary>
        ///  Tables drafting standard
        /// </summary>
        IXTablesDraftingStandardOptions TablesDraftingStandard { get; }

        /// <summary>
        /// Views drafting standard
        /// </summary>
        IXViewsDraftingStandardOptions ViewsDraftingStandard { get; }

        /// <summary>
        ///  Sheet Metal drafting standard
        /// </summary>
        IXSheetMetalDraftingStandardOptions SheetMetalDraftingStandard { get; }
    }

    /// <summary>
    /// Annotations drafting standard options
    /// </summary>
    public interface IXAnnotationsDraftingStandardOptions 
    {
        /// <summary>
        /// Fond of annotations
        /// </summary>
        IFont TextFont { get; set; }
    }

    /// <summary>
    /// Dimensions drafting standard options
    /// </summary>
    public interface IXDimensionsDraftingStandardOptions
    {
        /// <summary>
        /// Font of text format
        /// </summary>
        IFont TextFont { get; set; }
    }

    /// <summary>
    /// Tables drafting standard options
    /// </summary>
    public interface IXTablesDraftingStandardOptions
    {
        /// <summary>
        /// Font of text format
        /// </summary>
        IFont TextFont { get; set; }
    }

    /// <summary>
    /// Views drafting standard options
    /// </summary>
    public interface IXViewsDraftingStandardOptions
    {
        /// <summary>
        /// Font of text format
        /// </summary>
        IFont TextFont { get; set; }
    }

    /// <summary>
    /// Sheet Metal drafting standard options
    /// </summary>
    public interface IXSheetMetalDraftingStandardOptions
    {
        /// <summary>
        /// Font of text format
        /// </summary>
        IFont TextFont { get; set; }
    }

    /// <summary>
    /// Detailing options of the drawing
    /// </summary>
    public interface IXDrawingDetailingOptions 
    {
        /// <summary>
        /// Display cosmetic threads
        /// </summary>
        bool DisplayCosmeticThreads { get; set; }

        /// <summary>
        /// Auto insert center marks for slots
        /// </summary>
        bool AutoInsertCenterMarksForSlots { get; set; }

        /// <summary>
        /// Auto insert center marks for fillets
        /// </summary>
        bool AutoInsertCenterMarksForFillets { get; set; }

        /// <summary>
        /// Auto insert center marks for holes
        /// </summary>
        bool AutoInsertCenterMarksForHoles { get; set; }

        /// <summary>
        /// Auto insert dowel symbols
        /// </summary>
        bool AutoInsertDowelSymbols { get; set; }
    }

    /// <summary>
    /// Drawing specific options
    /// </summary>
    public interface IXDrawingOptions : IXDocumentOptions 
    {
        /// <summary>
        /// Detailing options of the drawing
        /// </summary>
        IXDrawingDetailingOptions Detailing { get; }
    }
}
