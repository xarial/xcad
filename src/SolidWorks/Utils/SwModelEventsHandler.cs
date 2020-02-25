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
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal abstract class SwModelEventsHandler<TDel> : EventsHandler<TDel>
        where TDel : Delegate
    {
        private readonly IModelDoc2 m_Model;

        internal SwModelEventsHandler(IModelDoc2 model) 
        {
            m_Model = model;
        }

        protected override void SubscribeEvents()
        {
            switch (m_Model) 
            {
                case PartDoc part:
                    SubscribePartEvents(part);
                    break;

                case AssemblyDoc assm:
                    SubscribeAssemblyEvents(assm);
                    break;

                case DrawingDoc drw:
                    SubscribeDrawingEvents(drw);
                    break;
            }
        }

        protected override void UnsubscribeEvents()
        {
            switch (m_Model)
            {
                case PartDoc part:
                    UnsubscribePartEvents(part);
                    break;

                case AssemblyDoc assm:
                    UnsubscribeAssemblyEvents(assm);
                    break;

                case DrawingDoc drw:
                    UnsubscribeDrawingEvents(drw);
                    break;
            }
        }

        protected abstract void SubscribePartEvents(PartDoc part);
        protected abstract void SubscribeAssemblyEvents(AssemblyDoc assm);
        protected abstract void SubscribeDrawingEvents(DrawingDoc drw);

        protected abstract void UnsubscribePartEvents(PartDoc part);
        protected abstract void UnsubscribeAssemblyEvents(AssemblyDoc assm);
        protected abstract void UnsubscribeDrawingEvents(DrawingDoc drw);
    }
}
