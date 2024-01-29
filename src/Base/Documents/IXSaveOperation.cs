//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
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
    /// Options to export splines in <see cref="IXDxfDwgSaveOperation.SplineExportOptions"/>
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
        Polylines
    }

    /// <summary>
    /// Save options for DXF/DWG format
    /// </summary>
    public interface IXDxfDwgSaveOperation : IXDrawingSaveOperation
    {   
        /// <summary>
        /// File path to a layers map file
        /// </summary>
        string LayersMapFilePath { get; set; }

        /// <summary>
        /// True to include hidden layers, False to only export visible layers
        /// </summary>
        bool ExportHiddentLayers { get; set; }

        /// <summary>
        /// Options to export splines
        /// </summary>
        SplineExportOptions_e SplineExportOptions { get; set; }
    }
}