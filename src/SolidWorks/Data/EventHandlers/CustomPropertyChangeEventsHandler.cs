//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Data.Delegates;
using Xarial.XCad.SolidWorks.Data.Helpers;
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.SolidWorks.Data.EventHandlers
{
    internal class CustomPropertyChangeEventsHandler : EventsHandler<PropertyValueChangedDelegate>
    {
        private readonly IModelDoc2 m_Model;
        private readonly SwCustomProperty m_Prp;
        
        internal CustomPropertyChangeEventsHandler(IModelDoc2 model, SwCustomProperty prp) 
        {
            m_Model = model;
            m_Prp = prp;
        }

        protected override void SubscribeEvents()
        {
            switch (m_Model)
            {
                case PartDoc part:
                    part.ChangeCustomPropertyNotify += OnChangeCustomPropertyNotify;
                    break;

                case AssemblyDoc assm:
                    assm.ChangeCustomPropertyNotify += OnChangeCustomPropertyNotify;
                    break;

                case DrawingDoc drw:
                    drw.ChangeCustomPropertyNotify += OnChangeCustomPropertyNotify;
                    break;
            }
        }

        protected override void UnsubscribeEvents()
        {
            switch (m_Model)
            {
                case PartDoc part:
                    part.ChangeCustomPropertyNotify -= OnChangeCustomPropertyNotify;
                    break;

                case AssemblyDoc assm:
                    assm.ChangeCustomPropertyNotify -= OnChangeCustomPropertyNotify;
                    break;

                case DrawingDoc drw:
                    drw.ChangeCustomPropertyNotify -= OnChangeCustomPropertyNotify;
                    break;
            }
        }

        private int OnChangeCustomPropertyNotify(string propName, string Configuration, string oldValue, string NewValue, int valueType)
        {
            Filter(propName, Configuration, NewValue);

            return S_OK;
        }

        protected void Filter(string prpName, string confName, string newValue)
        {
            if (string.Equals(prpName, m_Prp.Name, StringComparison.CurrentCultureIgnoreCase)
                            && string.Equals(confName, m_Prp.ConfigurationName, StringComparison.CurrentCultureIgnoreCase))
            {
                Delegate?.Invoke(m_Prp, newValue);
            }
        }
    }

    internal class CustomPropertyChangeEventsHandlerFromSw2017 : CustomPropertyChangeEventsHandler
    {
        private CustomPropertiesEventsHelper m_EventsHelper;

        internal CustomPropertyChangeEventsHandlerFromSw2017(CustomPropertiesEventsHelper eventsHelper, IModelDoc2 model, SwCustomProperty prp) 
            : base(model, prp)
        {
            m_EventsHelper = eventsHelper;
        }

        protected override void SubscribeEvents()
        {
            m_EventsHelper.CustomPropertiesModified += OnCustomPropertiesModified;
        }

        protected override void UnsubscribeEvents()
        {
            m_EventsHelper.CustomPropertiesModified -= OnCustomPropertiesModified;
        }

        private void OnCustomPropertiesModified(CustomPropertyModifyData[] modifications)
        {
            if (modifications != null)
            {
                foreach (var mod in modifications.Where(x => x.Action == CustomPropertyChangeAction_e.Modify))
                {
                    Filter(mod.Name, mod.ConfigurationName, mod.Value);
                }
            }
        }
    }
}
