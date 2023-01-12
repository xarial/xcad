//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
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
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Features.Delegates;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    internal class ComponentInsertedEventsHandler : SwModelEventsHandler<ComponentInsertedDelegate>
    {
        private readonly SwAssembly m_Assm;

        internal ComponentInsertedEventsHandler(SwAssembly assm, ISwApplication app) : base(assm, app)
        {
            m_Assm = assm;
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.AddItemNotify += OnAddItemNotify;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.AddItemNotify -= OnAddItemNotify;
        }

        private int OnAddItemNotify(int entityType, string itemName)
        {
            if (entityType == (int)swNotifyEntityType_e.swNotifyComponent || entityType == (int)swNotifyEntityType_e.swNotifyComponentInternal)
            {
                Delegate?.Invoke(m_Assm, ((SwAssemblyConfiguration)m_Assm.Configurations.Active).Components[itemName]);
            }

            return HResult.S_OK;
        }
    }
}
