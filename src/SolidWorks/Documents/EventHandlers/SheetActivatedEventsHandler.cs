//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
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

        internal SheetActivatedEventsHandler(SwDrawing drw, ISwApplication app) : base(drw, app)
        {
            m_Drw = drw;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.ActivateSheetPostNotify += OnActivateSheetPostNotify;
            drw.AddItemNotify += OnAddItemNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.ActivateSheetPostNotify -= OnActivateSheetPostNotify;
            drw.AddItemNotify -= OnAddItemNotify;
        }
        
        private int OnActivateSheetPostNotify(string sheetName)
        {
            Delegate?.Invoke(m_Drw, m_Drw.Sheets[sheetName]);
            return HResult.S_OK;
        }

        private int OnAddItemNotify(int entityType, string itemName)
        {
            if (entityType == (int)swNotifyEntityType_e.swNotifyDrawingSheet)
            {
                var sheet = m_Drw.Sheets[itemName];

                if (m_Drw.Sheets.Active.Equals(sheet))
                {
                    Delegate?.Invoke(m_Drw, sheet);
                }
            }

            return HResult.S_OK;
        }
    }
}
