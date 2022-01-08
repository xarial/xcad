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
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Documents.EventHandlers
{
    internal class NewSelectionEventHandler : SwModelEventsHandler<NewSelectionDelegate>
    {
        private IModelDoc2 Model => m_Doc.Model;
        private ISelectionMgr SelMgr => Model.ISelectionManager;

        internal NewSelectionEventHandler(SwDocument doc, ISwApplication app) : base(doc, app)
        {
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
            var selIndex = SelMgr.GetSelectedObjectCount2(-1);

            if (selIndex > 0)
            {
                var lastSelObj = SelMgr.GetSelectedObject6(selIndex, -1);
                var obj = m_Doc.CreateObjectFromDispatch<ISwSelObject>(lastSelObj);
                Delegate?.Invoke(m_Doc, obj);
            }

            return HResult.S_OK;
        }
    }
}
