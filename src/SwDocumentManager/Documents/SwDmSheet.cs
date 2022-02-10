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
using System.Threading;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.UI;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmSheet : IXSheet, ISwDmObject
    {
        ISwDMSheet Sheet { get; }
    }

    internal class SwDmSheet : SwDmObject, ISwDmSheet
    {
        #region Not Supported
        public Scale Scale { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        #endregion

        public string Name
        {
            get => Sheet.Name;
            set => Sheet.Name = value;
        }

        public IXDrawingViewRepository DrawingViews => m_DrawingViewsLazy.Value;

        public IXImage Preview 
        {
            get 
            {
                SwDmPreviewError previewErr;
                var imgBytes = ((ISwDMSheet2)Sheet).GetPreviewPNGBitmapBytes(out previewErr) as byte[];

                if (previewErr == SwDmPreviewError.swDmPreviewErrorNone)
                {
                    return new BaseImage(imgBytes);
                }
                else
                {
                    throw new Exception($"Failed to extract preview from the sheet: {previewErr}");
                }
            }
        }

        public bool IsCommitted => true;

        public ISwDMSheet Sheet { get; }
        
        private readonly Lazy<SwDmDrawingViewsCollection> m_DrawingViewsLazy;

        internal SwDmSheet(ISwDMSheet sheet, SwDmDrawing drw) : base(sheet)
        {
            Sheet = sheet;
            m_DrawingViewsLazy = new Lazy<SwDmDrawingViewsCollection>(() => new SwDmDrawingViewsCollection(this, drw));
        }

        public void Commit(CancellationToken cancellationToken)
             => throw new NotSupportedException();
    }
}
