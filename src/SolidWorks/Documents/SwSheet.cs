//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwSheet : ISwObject, IXSheet
    {
        ISheet Sheet { get; }
    }

    internal class SwSheet : SwObject, ISwSheet
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
        public bool IsCommitted => true;

        public override object Dispatch => Sheet;

        public IXImage Preview
            => PictureDispUtils.PictureDispToXImage(m_Drawing.App.Sw.GetPreviewBitmap(m_Doc.Path, Name));

        internal SwSheet(SwDrawing draw, ISheet sheet) : base(sheet, draw)
        {
            m_Drawing = draw;
            Sheet = sheet;
            DrawingViews = new SwDrawingViewsCollection(draw, sheet);
        }

        public void Commit(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
