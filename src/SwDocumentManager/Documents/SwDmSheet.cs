﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Features;
using Xarial.XCad.UI;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmSheet : IXSheet, ISwDmObject
    {
        ISwDMSheet Sheet { get; }
    }

    internal class SwDmSheet : SwDmSelObject, ISwDmSheet
    {
        #region Not Supported
        public IXSheet Clone(IXDrawing targetDrawing) => throw new NotSupportedException();
        public IXSketch2D Sketch => throw new NotSupportedException();
        #endregion Not Supported

        public string Name
        {
            get => Sheet.Name;
            set => Sheet.Name = value;
        }

        public IXDrawingViewRepository DrawingViews => m_DrawingViewsLazy.Value;

        public Scale Scale
        {
            get
            {
                if (((ISwDMDocument13)m_Drawing.Document).GetSheetProperties(Name, out object prps) == (int)swSheetPropertiesResult.swSheetProperties_TRUE)
                {
                    var prpsArr = (double[])prps;

                    return new Scale(prpsArr[3], prpsArr[4]);
                }
                else
                {
                    throw new Exception("Failed to read sheet properties");
                }
            }
            set => throw new NotSupportedException();
        }

        public PaperSize PaperSize
        {
            get
            {
                if (((ISwDMDocument13)m_Drawing.Document).GetSheetProperties(Name, out object prps) == (int)swSheetPropertiesResult.swSheetProperties_TRUE)
                {
                    var prpsArr = (double[])prps;

                    const int swDwgPapersUserDefined = 12;

                    var paperSize = Convert.ToInt32(prpsArr[0]);

                    var standardPaperSize = paperSize == swDwgPapersUserDefined ? default(StandardPaperSize_e?) : (StandardPaperSize_e)paperSize;

                    return new PaperSize(standardPaperSize, prpsArr[1], prpsArr[2]);
                }
                else
                {
                    throw new Exception("Failed to read sheet properties");
                }
            }
            set => throw new NotSupportedException();
        }

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

        public ISwDMSheet Sheet { get; }

        private readonly Lazy<SwDmDrawingViewsCollection> m_DrawingViewsLazy;

        private readonly SwDmDrawing m_Drawing;

        internal SwDmSheet(ISwDMSheet sheet, SwDmDrawing drw) : base(sheet, drw.OwnerApplication, drw)
        {
            Sheet = sheet;
            m_Drawing = drw;

            m_DrawingViewsLazy = new Lazy<SwDmDrawingViewsCollection>(() => new SwDmDrawingViewsCollection(this, drw));
        }
    }
}
