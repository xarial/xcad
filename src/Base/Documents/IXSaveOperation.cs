//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Base;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Save as operation for the document
    /// </summary>
    /// <remarks>Created via <see cref="IXDocument.PreCreateSaveAsOperation(string)"/></remarks>
    /// <exception cref="Exceptions.SaveDocumentFailedException"/>
    public interface IXSaveOperation : IXTransaction
    {
        /// <summary>
        /// Output file path
        /// </summary>
        string FilePath { get; }
    }

    /// <summary>
    /// Save operation of <see cref="IXDrawing"/> files
    /// </summary>
    public interface IXDrawingSaveOperation : IXSaveOperation
    {
        /// <summary>
        /// Sheets to export
        /// </summary>
        /// <remarks>If not specified all sheets are exported</remarks>
        IXSheet[] Sheets { get; set; }
    }

    /// <summary>
    /// Save operation of <see cref="IXDocument3D"/> files
    /// </summary>
    public interface IXDocument3DSaveOperation : IXSaveOperation 
    {
        /// <summary>
        /// Bodies to export
        /// </summary>
        /// <remarks>If not specified all bodies are exported</remarks>
        IXBody[] Bodies { get; set; }
    }

    /// <summary>
    /// Step format type
    /// </summary>
    public enum StepFormat_e
    {
        /// <summary>
        /// STEP AP 203
        /// </summary>
        Ap203,

        /// <summary>
        /// STEP AP 214
        /// </summary>
        Ap214,

        /// <summary>
        /// STEP AP 242
        /// </summary>
        Ap242
    }

    /// <summary>
    /// Save options of step format
    /// </summary>
    public interface IXStepSaveOperation : IXDocument3DSaveOperation
    {
        /// <summary>
        /// Step format
        /// </summary>
        StepFormat_e Format { get; set; }
    }

    /// <summary>
    /// IFC file format
    /// </summary>
    public enum IfcFormat_e
    {
        /// <summary>
        /// IFC 2x3
        /// </summary>
        Ifc2x3,

        /// <summary>
        /// IFC 4
        /// </summary>
        Ifc4
    }

    /// <summary>
    /// Save options of step format
    /// </summary>
    public interface IXIfcSaveOperation : IXDocument3DSaveOperation
    {
        /// <summary>
        /// IFC format
        /// </summary>
        IfcFormat_e Format { get; set; }

        /// <summary>
        /// Export units
        /// </summary>
        Length_e Units { get; set; }
    }

    /// <summary>
    /// Save options for PDF format
    /// </summary>
    public interface IXPdfSaveOperation : IXSaveOperation
    {
    }

    /// <summary>
    /// Save options for PDF format in 3D document
    /// </summary>
    public interface IXDocument3DPdfSaveOperation : IXDocument3DSaveOperation, IXPdfSaveOperation
    {
        /// <summary>
        /// Save PDF as 3D PDF
        /// </summary>
        bool Pdf3D { get; set; }
    }

    /// <summary>
    /// Save options for PDF format in drawing document
    /// </summary>
    public interface IXDrawingPdfSaveOperation : IXPdfSaveOperation, IXDrawingSaveOperation
    {

    }

    /// <summary>
    /// Options to export splines in <see cref="IXDxfDwgDrawingSaveOperation.SplineExportOptions"/>
    /// </summary>
    public enum SplineExportOptions_e 
    {
        /// <summary>
        /// Exports splines as splines
        /// </summary>
        Splines,

        /// <summary>
        /// Exports splines as polylines
        /// </summary>
        Polylines,

        /// <summary>
        /// Exports splines as tangent arcs
        /// </summary>
        TangentArcs
    }

    /// <summary>
    /// Save options for DXF/DWG format
    /// </summary>
    public interface IXDxfDwgSaveOperation : IXSaveOperation
    {
        /// <summary>
        /// File path to a configuration file
        /// </summary>
        /// <remarks>This file contains configuration of export, this usually includes layers map</remarks>
        string ConfigurationFilePath { get; set; }

        /// <summary>
        /// Options to export splines
        /// </summary>
        SplineExportOptions_e SplineExportOptions { get; set; }

    }

    /// <summary>
    /// Save options for DXF/DWG format from drawing
    /// </summary>
    public interface IXDxfDwgDrawingSaveOperation : IXDrawingSaveOperation, IXDxfDwgSaveOperation
    {   
        /// <summary>
        /// True to include hidden layers, False to only export visible layers
        /// </summary>
        bool ExportHiddenLayers { get; set; }
    }

    /// <summary>
    /// Save options for <see cref="Features.IXFlatPattern"/>
    /// </summary>
    public interface IFlatPatternSaveOperation : IXDxfDwgSaveOperation
    {
        /// <summary>
        /// Flat pattern view options
        /// </summary>
        FlatPatternViewOptions_e ViewOptions { get; set; }
    }
}