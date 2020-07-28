﻿using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Documents.EventHandlers
{
    internal class DocumentRebuildEventsHandler : SwModelEventsHandler<DocumentRebuildDelegate>
    {
        private SwDocument m_Doc;

        internal DocumentRebuildEventsHandler(SwDocument doc) : base(doc.Model)
        {
            m_Doc = doc;
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.RegenPostNotify2 += OnRegenPostNotify;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.RegenPostNotify += OnDrwRegenPostNotify;
        }

        protected override void SubscribePartEvents(PartDoc part)
        {
            part.RegenPostNotify2 += OnRegenPostNotify;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.RegenPostNotify2 -= OnRegenPostNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.RegenPostNotify -= OnDrwRegenPostNotify;
        }

        protected override void UnsubscribePartEvents(PartDoc part)
        {
            part.RegenPostNotify2 -= OnRegenPostNotify;
        }

        private int OnRegenPostNotify(object stopFeature)
        {
            Delegate?.Invoke(m_Doc);
            return S_OK;
        }

        private int OnDrwRegenPostNotify()
        {
            Delegate?.Invoke(m_Doc);
            return S_OK;
        }
    }
}