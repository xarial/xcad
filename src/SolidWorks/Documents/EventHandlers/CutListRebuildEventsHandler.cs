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

namespace Xarial.XCad.SolidWorks.Documents.EventHandlers
{
    internal class CutListRebuildEventsHandler : SwModelEventsHandler<CutListRebuildDelegate>
    {
        private readonly ISwPart m_Part;

        internal CutListRebuildEventsHandler(SwPart part, ISwApplication app) : base(part, app)
        {
            m_Part = part;
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
            Delegate?.Invoke(m_Part);
            return HResult.S_OK;
        }
    }
}
