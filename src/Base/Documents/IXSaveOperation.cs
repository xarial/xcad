//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;

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
    public interface IXStepSaveOperation : IXSaveOperation
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
    public interface IXDocument3DPdfSaveOperation : IXPdfSaveOperation 
    {
        /// <summary>
        /// Save PDF as 3D PDF
        /// </summary>
        bool Pdf3D { get; set; }
    }

    /// <summary>
    /// Save options for PDF format in drawing document
    /// </summary>
    public interface IXDrawingPdfSaveOperation : IXPdfSaveOperation
    {
        /// <summary>
        /// Sheets to export
        /// </summary>
        IXSheet[] Sheets { get; set; }
    }

    /// <summary>
    /// Save options for DXF/DWG format
    /// </summary>
    public interface IXDxfDwgSaveOperation : IXSaveOperation
    {   
        /// <summary>
        /// File path to a layers map file
        /// </summary>
        string LayersMapFilePath { get; set; }
    }
}