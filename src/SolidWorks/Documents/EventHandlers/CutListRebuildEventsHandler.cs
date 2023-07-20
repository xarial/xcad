//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Features.Delegates;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Documents.EventHandlers
{
    internal class CutListRebuildEventsHandler : SwModelEventsHandler<CutListRebuildDelegate>
    {
        private readonly ISwPart m_Part;
        private readonly SwCutListItemCollection m_CutLists;

        internal CutListRebuildEventsHandler(SwPart part, SwCutListItemCollection cutLists) : base(part, part.OwnerApplication)
        {
            m_Part = part;
            m_CutLists = cutLists;
        }
        
        protected override void SubscribePartEvents(PartDoc part)
        {
            part.WeldmentCutListUpdatePostNotify += OnWeldmentCutListUpdatePostNotify;
        }
        
        protected override void UnsubscribePartEvents(PartDoc part)
        {
            part.WeldmentCutListUpdatePostNotify -= OnWeldmentCutListUpdatePostNotify;
        }

        private int OnWeldmentCutListUpdatePostNotify()
        {
            Delegate?.Invoke(m_CutLists);
            return HResult.S_OK;
        }
    }
}
