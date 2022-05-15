//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmDrawing : ISwDmDocument, IXDrawing
    {
        new ISwDmSheetCollection Sheets { get; }
    }

    internal class SwDmDrawing : SwDmDocument, ISwDmDrawing
    {
        IXSheetRepository IXDrawing.Sheets => Sheets;

        private readonly Lazy<SwDmSheetCollection> m_SheetsLazy;

        public SwDmDrawing(ISwDmApplication dmApp, ISwDMDocument doc, bool isCreated,
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
