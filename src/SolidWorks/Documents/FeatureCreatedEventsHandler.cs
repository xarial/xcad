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
using System.Threading.Tasks;
using Xarial.XCad.Features.Delegates;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    internal class FeatureCreatedEventsHandler : SwModelEventsHandler<FeatureCreatedDelegate>
    {
        public FeatureCreatedEventsHandler(SwDocument doc, ISwApplication app) : base(doc, app)
        {
        }

        protected override void SubscribePartEvents(PartDoc part)
        {
            part.AddItemNotify += OnAddItemNotify;
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.AddItemNotify += OnAddItemNotify;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.AddItemNotify += OnAddItemNotify;
        }

        protected override void UnsubscribePartEvents(PartDoc part)
        {
            part.AddItemNotify -= OnAddItemNotify;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.AddItemNotify -= OnAddItemNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.AddItemNotify -= OnAddItemNotify;
        }

        private int OnAddItemNotify(int entityType, string itemName)
        {
            if (entityType == (int)swNotifyEntityType_e.swNotifyFeature)
            {
                Delegate?.Invoke(m_Doc, m_Doc.Features[itemName]);
            }

            return HResult.S_OK;
        }
    }
}
