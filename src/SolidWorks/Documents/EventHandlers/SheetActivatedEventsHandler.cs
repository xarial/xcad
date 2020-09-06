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
        private SwDrawing m_Doc;

        internal SheetActivatedEventsHandler(SwDrawing doc) : base(doc.Model)
        {
            m_Doc = doc;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.ActivateSheetPostNotify += OnActivateSheetPostNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.ActivateSheetPostNotify -= OnActivateSheetPostNotify;
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            throw new NotSupportedException();
        }

        protected override void SubscribePartEvents(PartDoc part)
        {
            throw new NotSupportedException();
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            throw new NotSupportedException();
        }

        protected override void UnsubscribePartEvents(PartDoc part)
        {
            throw new NotSupportedException();
        }

        private int OnActivateSheetPostNotify(string sheetName)
        {
            Delegate?.Invoke(m_Doc, m_Doc.Sheets[sheetName]);
            return S_OK;
        }
    }
}
