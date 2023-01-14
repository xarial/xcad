//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Features.Delegates;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    internal class SheetCreatedEventsHandler : SwModelEventsHandler<SheetCreatedDelegate>
    {
        private readonly SwDrawing m_Drw;

        public SheetCreatedEventsHandler(SwDrawing draw, ISwApplication app) : base(draw, app)
        {
            m_Drw = draw;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.AddItemNotify += OnAddItemNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.AddItemNotify -= OnAddItemNotify;
        }

        private int OnAddItemNotify(int entityType, string itemName)
        {
            if (entityType == (int)swNotifyEntityType_e.swNotifyDrawingSheet)
            {
                Delegate?.Invoke(m_Drw, m_Drw.Sheets[itemName]);
            }

            return HResult.S_OK;
        }
    }
}
