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

namespace Xarial.XCad.SolidWorks.Data.EventHandlers
{
    internal class StreamWriteAvailableEventsHandler : SwModelEventsHandler<DataStoreAvailableDelegate>
    {
        private readonly SwDocument m_Doc;

        internal StreamWriteAvailableEventsHandler(SwDocument doc) : base(doc.Model)
        {
            m_Doc = doc;
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.SaveToStorageNotify += OnWriteToStreamNotify;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.SaveToStorageNotify += OnWriteToStreamNotify;
        }

        protected override void SubscribePartEvents(PartDoc part)
        {
            part.SaveToStorageNotify += OnWriteToStreamNotify;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.SaveToStorageNotify -= OnWriteToStreamNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.SaveToStorageNotify -= OnWriteToStreamNotify;
        }

        protected override void UnsubscribePartEvents(PartDoc part)
        {
            part.SaveToStorageNotify -= OnWriteToStreamNotify;
        }

        private int OnWriteToStreamNotify()
        {
            Delegate?.Invoke(m_Doc);
            return S_OK;
        }
    }
}
