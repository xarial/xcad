﻿using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Documents.EventHandlers
{
    internal class ConfigurationActivatedEventsHandler : SwModelEventsHandler<ConfigurationActivatedDelegate>
    {
        private SwDocument3D m_Doc;

        internal ConfigurationActivatedEventsHandler(SwDocument3D doc) : base(doc.Model)
        {
            m_Doc = doc;
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.ConfigurationChangeNotify += OnConfigurationChangeNotify;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc drw)
        {
            throw new NotSupportedException();
        }

        protected override void SubscribePartEvents(PartDoc part)
        {
            part.ConfigurationChangeNotify += OnConfigurationChangeNotify;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.ConfigurationChangeNotify -= OnConfigurationChangeNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc drw)
        {
            throw new NotSupportedException();
        }

        protected override void UnsubscribePartEvents(PartDoc part)
        {
            part.ConfigurationChangeNotify -= OnConfigurationChangeNotify;
        }

        private int OnConfigurationChangeNotify(string configurationName, object obj, int objectType, int changeType)
        {
            const int POST_NOTIFICATION = 11;

            if (changeType == POST_NOTIFICATION)
            {
                Delegate?.Invoke(m_Doc.Configurations[configurationName]);
            }

            return S_OK;
        }
    }
}
