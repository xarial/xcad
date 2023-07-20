//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Documents.Structures;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the drawing (2D draft)
    /// </summary>
    public interface IXDrawing : IXDocument
    {
        /// <summary>
        /// Sheets on this drawing
        /// </summary>
        IXSheetRepository Sheets { get; }

        /// <summary>
        /// Drawing layers
        /// </summary>
        IXLayerRepository Layers { get; }

        /// <summary>
        /// Drawing specific options
        /// </summary>
        new IXDrawingOptions Options { get; }

        /// <summary>
        /// <see cref="IXDrawing"/> specific save as operation
        /// </summary>
        new IXDrawingSaveOperation PreCreateSaveAsOperation(string filePath);
    }
}