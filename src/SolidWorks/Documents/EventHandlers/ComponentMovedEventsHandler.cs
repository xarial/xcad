//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Documents.EventHandlers
{
    internal class ComponentMovedEventsHandler : SwModelEventsHandler<ComponentMovedDelegate>
    {
        private readonly SwComponent m_Comp;

        internal ComponentMovedEventsHandler(SwComponent comp, SwDocument3D doc, ISwApplication app) : base(doc, app)
        {
            m_Comp = comp;
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.ComponentMoveNotify2 += OnComponentMove;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.ComponentMoveNotify2 -= OnComponentMove;
        }

        private int OnComponentMove(ref object Components)
        {
            if (Components is object[]) 
            {
                if (((object[])Components).Contains(m_Comp.Component))
                {
                    Delegate?.Invoke(m_Comp);
                }
            }
            
            return HResult.S_OK;
        }
    }
}
