//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmDrawing : ISwDmDocument, IXDrawing
    {
        new ISwDmSheetCollection Sheets { get; }
    }

    internal class SwDmDrawing : SwDmDocument, ISwDmDrawing
    {
        #region Not Supported
        
        IXDrawingOptions IXDrawing.Options => throw new NotSupportedException();
        IXDrawingSaveOperation IXDrawing.PreCreateSaveAsOperation(string filePath) => throw new NotSupportedException();
        public IXLayerRepository Layers => throw new NotSupportedException();

        #endregion

        IXSheetRepository IXDrawing.Sheets => Sheets;

        private readonly Lazy<SwDmSheetCollection> m_SheetsLazy;

        public SwDmDrawing(SwDmApplication dmApp, ISwDMDocument doc, bool isCreated,
            Action<ISwDmDocument> createHandler, Action<ISwDmDocument> closeHandler,
            bool? isReadOnly)
            : base(dmApp, doc, isCreated, createHandler, closeHandler, isReadOnly)
        {
            m_SheetsLazy = new Lazy<SwDmSheetCollection>(() => new SwDmSheetCollection(this));
        }

        public ISwDmSheetCollection Sheets => m_SheetsLazy.Value;

        protected override bool IsDocumentTypeCompatible(SwDmDocumentType docType) => docType == SwDmDocumentType.swDmDocumentDrawing;        
    }
}
