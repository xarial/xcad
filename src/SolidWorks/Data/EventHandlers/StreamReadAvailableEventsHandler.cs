//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Data.EventHandlers
{
    internal class StreamReadAvailableEventsHandler : SwModelEventsHandler<DataStoreAvailableDelegate>
    {
        private bool m_Is3rdPartyStreamLoaded;
        private readonly SwDocument m_Doc;

        internal StreamReadAvailableEventsHandler(SwDocument doc) : base(doc.Model)
        {
            m_Doc = doc;
            m_Is3rdPartyStreamLoaded = false;
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.LoadFromStorageNotify += OnLoadFromStreamNotify;
            SubscribeIdleEvent();
        }

        protected override void SubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.LoadFromStorageNotify += OnLoadFromStreamNotify;
            SubscribeIdleEvent();
        }

        protected override void SubscribePartEvents(PartDoc part)
        {
            part.LoadFromStorageNotify += OnLoadFromStreamNotify;
            SubscribeIdleEvent();
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.LoadFromStorageNotify -= OnLoadFromStreamNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.LoadFromStorageNotify -= OnLoadFromStreamNotify;
        }

        protected override void UnsubscribePartEvents(PartDoc part)
        {
            part.LoadFromStorageNotify -= OnLoadFromStreamNotify;
        }

        private int OnLoadFromStreamNotify()
        {
            //NOTE: by some reasons this event is triggered twice, adding the flag to avoid repetition
            return EnsureLoadFromStream();
        }

        private void SubscribeIdleEvent()
        {
            //NOTE: load from storage notification is not always raised
            //it is not raised when model is loaded with assembly, it won't be also raised if the document already loaded
            //as a workaround force call loading within the idle notification
            (m_Doc.App as SldWorks).OnIdleNotify += OnIdleHandleThirdPartyStorageNotify;
        }

        private int OnIdleHandleThirdPartyStorageNotify()
        {
            EnsureLoadFromStream();

            //only need to handle loading one time
            (m_Doc.App as SldWorks).OnIdleNotify -= OnIdleHandleThirdPartyStorageNotify;

            return S_OK;
        }

        private int EnsureLoadFromStream()
        {
            if (!m_Is3rdPartyStreamLoaded)
            {
                m_Is3rdPartyStreamLoaded = true;
                Delegate?.Invoke(m_Doc);
            }

            return S_OK;
        }
    }
}
