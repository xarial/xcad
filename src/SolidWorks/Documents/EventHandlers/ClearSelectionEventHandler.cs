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
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Documents.EventHandlers
{
    internal class ClearSelectionEventHandler : SwModelEventsHandler<ClearSelectionDelegate>
    {
        private readonly SwDocument m_Doc;

        internal ClearSelectionEventHandler(SwDocument doc) : base(doc)
        {
            m_Doc = doc;
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.ClearSelectionsNotify += OnClearSelections;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.ClearSelectionsNotify += OnClearSelections;
        }

        protected override void SubscribePartEvents(PartDoc part)
        {
            part.ClearSelectionsNotify += OnClearSelections;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.ClearSelectionsNotify -= OnClearSelections;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.ClearSelectionsNotify -= OnClearSelections;
        }

        protected override void UnsubscribePartEvents(PartDoc part)
        {
            part.ClearSelectionsNotify -= OnClearSelections;
        }

        private int OnClearSelections()
        {
            Delegate?.Invoke(m_Doc);
            return S_OK;
        }
    }
}
