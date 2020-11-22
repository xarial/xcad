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
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Documents.EventHandlers
{
    internal class SheetActivatedEventsHandler : SwModelEventsHandler<SheetActivatedDelegate>
    {
        private SwDrawing m_Drw;

        internal SheetActivatedEventsHandler(SwDrawing drw) : base(drw)
        {
            m_Drw = drw;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.ActivateSheetPostNotify += OnActivateSheetPostNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.ActivateSheetPostNotify -= OnActivateSheetPostNotify;
        }
        
        private int OnActivateSheetPostNotify(string sheetName)
        {
            Delegate?.Invoke(m_Drw, m_Drw.Sheets[sheetName]);
            return S_OK;
        }
    }
}
