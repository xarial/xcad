//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Documents.Exceptions;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwSheet : ISwSelObject, IXSheet
    {
        ISheet Sheet { get; }
    }

    internal class SwSheet : SwSelObject, ISwSheet
    {
        public ISheet Sheet { get; }
        private readonly SwDrawing m_Drawing;

        public string Name
        {
            get => Sheet.GetName();
            set 
            {
                Sheet.SetName(value);
            }
        }

        public IXDrawingViewRepository DrawingViews { get; }

        //TODO: implement creation of new sheets
        public override bool IsCommitted => true;

        public override object Dispatch => Sheet;

        public IXImage Preview
            => PictureDispUtils.PictureDispToXImage(OwnerApplication.Sw.GetPreviewBitmap(m_Drawing.Path, Name));

        internal SwSheet(ISheet sheet, SwDrawing draw, ISwApplication app) : base(sheet, draw, app)
        {
            m_Drawing = draw;
            Sheet = sheet;
            DrawingViews = new SwDrawingViewsCollection(draw, sheet);
        }

        public override void Commit(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    internal class UncommittedPreviewOnlySheet : ISwSelObject, IXSheet 
    {
        #region Not Supported
        
        public string Name { get => throw new UnloadedDocumentPreviewOnlySheetException(); set => throw new UnloadedDocumentPreviewOnlySheetException(); }
        public IXDrawingViewRepository DrawingViews => throw new UnloadedDocumentPreviewOnlySheetException();
        public void Commit(CancellationToken cancellationToken)
            => throw new UnloadedDocumentPreviewOnlySheetException();
        public void Select(bool append)
            => throw new UnloadedDocumentPreviewOnlySheetException();
        public void Serialize(Stream stream)
            => throw new UnloadedDocumentPreviewOnlySheetException();

        public object Dispatch => throw new UnloadedDocumentPreviewOnlySheetException();
        public bool IsSelected => throw new UnloadedDocumentPreviewOnlySheetException();
        public bool IsAlive => throw new UnloadedDocumentPreviewOnlySheetException();
        public ITagsManager Tags => throw new UnloadedDocumentPreviewOnlySheetException();

        #endregion

        private readonly ISwApplication m_App;
        private readonly SwDrawing m_Drw;

        internal UncommittedPreviewOnlySheet(SwDrawing drw, ISwApplication app) 
        {
            m_Drw = drw;
            m_App = app;
        }

        public IXImage Preview
            => PictureDispUtils.PictureDispToXImage(m_App.Sw.GetPreviewBitmap(m_Drw.Path, ""));

        public bool IsCommitted => false;

        public bool Equals(IXObject other) => this == other;
    }
}
