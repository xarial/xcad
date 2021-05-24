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
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal abstract class SwModelEventsHandler<TDel> : EventsHandler<TDel>
        where TDel : Delegate
    {
        protected readonly ISwDocument m_Doc;
        private IModelDoc2 m_Model => m_Doc.Model;

        internal SwModelEventsHandler(ISwDocument doc) 
        {
            m_Doc = doc;
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

        protected virtual void SubscribePartEvents(PartDoc part)
        {
        }

        protected virtual void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
        }

        protected virtual void SubscribeDrawingEvents(DrawingDoc drw)
        {
        }

        protected virtual void UnsubscribePartEvents(PartDoc part) 
        {
        }

        protected virtual void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
        }

        protected virtual void UnsubscribeDrawingEvents(DrawingDoc drw)
        {
        }
    }
}
