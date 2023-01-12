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
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Features.Delegates;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    internal class ComponentDeletingEventsHandler : SwModelEventsHandler<ComponentDeletingDelegate>
    {
        private readonly SwAssembly m_Assm;

        internal ComponentDeletingEventsHandler(SwAssembly assm, ISwApplication app) : base(assm, app)
        {
            m_Assm = assm;
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.DeleteItemPreNotify += OnDeleteItemPreNotify;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.DeleteItemPreNotify -= OnDeleteItemPreNotify;
        }

        private int OnDeleteItemPreNotify(int entityType, string itemName)
        {
            if (entityType == (int)swNotifyEntityType_e.swNotifyComponent || entityType == (int)swNotifyEntityType_e.swNotifyComponentInternal)
            {
                var args = new ItemDeleteArgs();
                Delegate?.Invoke(m_Assm, ((SwAssemblyConfiguration)m_Assm.Configurations.Active).Components[itemName], args);

                if (args.Cancel) 
                {
                    throw new NotSupportedException("Cancelling is not supported");
                }
            }

            return HResult.S_OK;
        }
    }
}
