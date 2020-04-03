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
    internal class NewSelectionEventHandler : SwModelEventsHandler<NewSelectionDelegate>
    {
        private readonly IModelDoc2 m_Model;
        private readonly ISelectionMgr m_SelMgr;

        internal NewSelectionEventHandler(SwDocument doc) : base(doc.Model)
        {
            m_Model = doc.Model;
            m_SelMgr = m_Model.ISelectionManager;
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.NewSelectionNotify += OnNewSelection;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.NewSelectionNotify += OnNewSelection;
        }

        protected override void SubscribePartEvents(PartDoc part)
        {
            part.NewSelectionNotify += OnNewSelection;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.NewSelectionNotify -= OnNewSelection;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.NewSelectionNotify -= OnNewSelection;
        }

        protected override void UnsubscribePartEvents(PartDoc part)
        {
            part.NewSelectionNotify -= OnNewSelection;
        }

        private int OnNewSelection()
        {
            var selIndex = m_SelMgr.GetSelectedObjectCount2(-1);

            if (selIndex > 0)
            {
                var lastSelObj = m_SelMgr.GetSelectedObject6(selIndex, -1);
                var obj = (SwSelObject)SwSelObject.FromDispatch(lastSelObj, m_Model, o => new SwSelObject(m_Model, o));
                Delegate?.Invoke(obj);
            }

            return S_OK;
        }
    }
}
