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
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Features.Delegates;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    internal class ComponentDeletedEventsHandler : SwModelEventsHandler<ComponentDeletedDelegate>
    {
        private readonly SwAssembly m_Assm;
        private readonly Dictionary<string, IXComponent> m_DeletingComps;

        private readonly IXLogger m_Logger;

        internal ComponentDeletedEventsHandler(SwAssembly assm, ISwApplication app, IXLogger logger) : base(assm, app)
        {
            m_Assm = assm;
            m_DeletingComps = new Dictionary<string, IXComponent>(StringComparer.CurrentCultureIgnoreCase);
            m_Logger = logger;
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.DeleteItemPreNotify += OnDeleteItemPreNotify;
            assm.DeleteItemNotify += OnDeleteItemNotify;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.DeleteItemPreNotify -= OnDeleteItemPreNotify;
            assm.DeleteItemNotify -= OnDeleteItemNotify;
        }

        private int OnDeleteItemPreNotify(int entityType, string itemName)
        {
            if (entityType == (int)swNotifyEntityType_e.swNotifyComponent || entityType == (int)swNotifyEntityType_e.swNotifyComponentInternal)
            {
                var comp = ((SwAssemblyConfiguration)m_Assm.Configurations.Active).Components[itemName];

                m_DeletingComps[itemName] = comp;
            }

            return HResult.S_OK;
        }

        private int OnDeleteItemNotify(int entityType, string itemName)
        {
            if (entityType == (int)swNotifyEntityType_e.swNotifyComponent || entityType == (int)swNotifyEntityType_e.swNotifyComponentInternal)
            {
                if (m_DeletingComps.TryGetValue(itemName, out var comp))
                {
                    m_DeletingComps.Remove(itemName);
                }
                else 
                {
                    m_Logger.Log($"Missing the pointer of the deleting component '{itemName}'");

                    throw new Exception($"Missing the pointer of the deleting component '{itemName}'");
                }

                Delegate?.Invoke(m_Assm, comp);
            }

            return HResult.S_OK;
        }
    }
}
