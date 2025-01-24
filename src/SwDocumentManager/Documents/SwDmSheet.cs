//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Features;
using Xarial.XCad.Toolkit;
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
        public IXSheetFormat Format => throw new NotSupportedException();
        public IXAnnotationRepository Annotations => throw new NotSupportedException();
        #endregion

        public string Name
        {
            get => Sheet.Name;
            set => Sheet.Name = value;
        }

        public IXDrawingViewRepository DrawingViews => m_DrawingViewsLazy.Value;

        public IXIdentifier Id => new XIdentifier(((ISwDMSheet3)Sheet).GetID());

        public Scale Scale 
        {
            get 
            {
                var prps = GetSheetProperties();

                return new Scale(prps[3], prps[4]);
            }
            set => throw new NotSupportedException(); 
        }
        
        public PaperSize PaperSize 
        {
            get
            {
                var prps = GetSheetProperties();

                const int swDwgPapersUserDefined = 12;

                var paperSize = Convert.ToInt32(prps[0]);

                var standardPaperSize = paperSize == swDwgPapersUserDefined ? default(StandardPaperSize_e?) : (StandardPaperSize_e)paperSize;

                return new PaperSize(standardPaperSize, prps[1], prps[2]);
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

        public string Template 
        {
            get 
            {
                var res = ((ISwDMDocument13)m_Drawing.Document).GetSheetFormatPath(Name, out var sheetFormatPath);

                if (res == (int)swSheetFormatPathResult.swSheetFormatPath_TRUE)
                {
                    return sheetFormatPath;
                }
                else 
                {
                    throw new Exception($"Failed to read sheet format: {res}");
                }
            }
            set => throw new NotSupportedException();
        }

        public ViewsProjectionType_e ViewsProjectionType
        {
            get
            {
                var prps = GetSheetProperties();

                var isFirstAngle = Convert.ToBoolean(prps[5]);

                if (isFirstAngle)
                {
                    return ViewsProjectionType_e.FirstAngle;
                }
                else
                {
                    return ViewsProjectionType_e.ThirdAngle;
                }
            }
            set => throw new NotSupportedException();
        }


        private readonly Lazy<SwDmDrawingViewsCollection> m_DrawingViewsLazy;

        private readonly SwDmDrawing m_Drawing;

        internal SwDmSheet(ISwDMSheet sheet, SwDmDrawing drw) : base(sheet, drw.OwnerApplication, drw)
        {
            Sheet = sheet;
            m_Drawing = drw;

            m_DrawingViewsLazy = new Lazy<SwDmDrawingViewsCollection>(() => new SwDmDrawingViewsCollection(this, drw));
        }

        private double[] GetSheetProperties()
        {
            var res = ((ISwDMDocument13)m_Drawing.Document).GetSheetProperties(Name, out object prps);

            if (res == (int)swSheetPropertiesResult.swSheetProperties_TRUE)
            {
                return (double[])prps;
            }
            else
            {
                throw new Exception($"Failed to read sheet properties: {res}");
            }
        }
    }
}
