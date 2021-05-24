//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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
    internal class StorageWriteAvailableEventsHandler : SwModelEventsHandler<DataStoreAvailableDelegate>
    {
        private readonly SwDocument m_Doc;

        internal StorageWriteAvailableEventsHandler(SwDocument doc) : base(doc)
        {
            m_Doc = doc;
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.SaveToStorageStoreNotify += OnWriteToStorageNotify;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.SaveToStorageStoreNotify += OnWriteToStorageNotify;
        }

        protected override void SubscribePartEvents(PartDoc part)
        {
            part.SaveToStorageStoreNotify += OnWriteToStorageNotify;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.SaveToStorageStoreNotify -= OnWriteToStorageNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.SaveToStorageStoreNotify -= OnWriteToStorageNotify;
        }

        protected override void UnsubscribePartEvents(PartDoc part)
        {
            part.SaveToStorageStoreNotify -= OnWriteToStorageNotify;
        }

        private int OnWriteToStorageNotify()
        {
            Delegate?.Invoke(m_Doc);
            return S_OK;
        }
    }
}
