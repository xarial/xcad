//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
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
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.SolidWorks.Data.EventHandlers
{
    internal class StorageReadAvailableEventsHandler : SwModelEventsHandler<DataStoreAvailableDelegate>
    {
        private bool m_Is3rdPartyStorageLoaded;

        internal StorageReadAvailableEventsHandler(SwDocument doc, ISwApplication app) : base(doc, app)
        {
            m_Is3rdPartyStorageLoaded = false;
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.LoadFromStorageStoreNotify += OnLoadFromStorageNotify;

            SubscribeIdleEvent();
        }

        protected override void SubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.LoadFromStorageStoreNotify += OnLoadFromStorageNotify;

            SubscribeIdleEvent();
        }

        protected override void SubscribePartEvents(PartDoc part)
        {
            part.LoadFromStorageStoreNotify += OnLoadFromStorageNotify;

            SubscribeIdleEvent();
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.LoadFromStorageStoreNotify -= OnLoadFromStorageNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.LoadFromStorageStoreNotify -= OnLoadFromStorageNotify;
        }

        protected override void UnsubscribePartEvents(PartDoc part)
        {
            part.LoadFromStorageStoreNotify -= OnLoadFromStorageNotify;
        }

        private int OnLoadFromStorageNotify()
        {
            return EnsureLoadFromStorage();
        }

        private void SubscribeIdleEvent()
        {
            //WORKAROUND: load from storage notification is not always raised
            //it is not raised when model is loaded with assembly, it won't be also raised if the document already loaded
            //as a workaround force call loading within the idle notification
            (m_App.Sw as SldWorks).OnIdleNotify += OnIdleHandleThirdPartyStorageNotify;
        }

        private int OnIdleHandleThirdPartyStorageNotify()
        {
            EnsureLoadFromStorage();

            //only need to handle loading one time
            (m_App.Sw as SldWorks).OnIdleNotify -= OnIdleHandleThirdPartyStorageNotify;

            return HResult.S_OK;
        }

        private int EnsureLoadFromStorage()
        {
            if (!m_Is3rdPartyStorageLoaded)
            {
                m_Is3rdPartyStorageLoaded = true;
                Delegate?.Invoke(m_Doc);
            }

            return HResult.S_OK;
        }
    }
}
