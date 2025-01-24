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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Delegates;
using Xarial.XCad.SolidWorks.Data.EventHandlers;
using Xarial.XCad.SolidWorks.Data.Exceptions;
using Xarial.XCad.SolidWorks.Data.Helpers;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.SolidWorks.Data
{
    /// <summary>
    /// SOLIDWORKS-specific custom property
    /// </summary>
    public interface ISwCustomProperty : IXProperty
    {
    }

    [DebuggerDisplay("{" +nameof(Name) + "} = {" + nameof(Value) + "} ({" + nameof(Expression) + "})")]
    internal class SwCustomProperty : SwObject, ISwCustomProperty
    {
        private string m_Name;
        private object m_TempValue;

        public string Name 
        {
            get => m_Name;
            set 
            {
                if (m_Name != value) 
                {
                    RenameProperty(value);
                }
            }
        }

        public object Value 
        {
            get
            {
                if (IsCommitted)
                {
                    TryReadProperty(out _, out object resVal);
                    return resVal;
                }
                else 
                {
                    return m_TempValue;
                }
            }
            set
            {
                if (IsCommitted)
                {
                    SetProperty(PrpMgr, Name, value);
                }
                else 
                {
                    m_TempValue = value;
                }
            }
        }

        public string Expression 
        {
            get 
            {
                if (IsCommitted)
                {
                    TryReadProperty(out string val, out _);
                    return val;
                }
                else
                {
                    return m_TempValue?.ToString();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    Value = value;
                }
                else 
                {
                    m_TempValue = value;
                }
            }
        }

        public event PropertyValueChangedDelegate ValueChanged 
        {
            add 
            {
                m_CustomPropertyChangeEventsHandler.Attach(value);
            }
            remove 
            {
                m_CustomPropertyChangeEventsHandler.Detach(value);
            }
        }

        private EventsHandler<PropertyValueChangedDelegate> m_CustomPropertyChangeEventsHandler;

        protected ICustomPropertyManager PrpMgr => m_PrpMgrFact.Invoke();

        public override bool IsCommitted => m_IsCommitted;
        public bool UseCached { get; set; }

        protected readonly ISwApplication m_App;

        private bool m_IsCommitted;

        private readonly Func<ICustomPropertyManager> m_PrpMgrFact;

        internal SwCustomProperty(Func<ICustomPropertyManager> prpMgrFact, string name, bool isCommited, SwDocument doc, SwApplication app) : base(null, doc, app)
        {
            m_PrpMgrFact = prpMgrFact;
            UseCached = true;
            m_Name = name;
            m_IsCommitted = isCommited;
            m_App = app;
        }

        internal void SetEventsHandler(EventsHandler<PropertyValueChangedDelegate> eventsHandler) 
        {
            m_CustomPropertyChangeEventsHandler = eventsHandler;
        }

        private void RenameProperty(string newName) 
        {
            m_Name = newName;

            if (IsCommitted)
            {
                throw new NotImplementedException();
            }
        }
        
        private void TryReadProperty(out string val, out object resVal)
        {
            string resValStr;

            bool prpExist;
            
            if (m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2018))
            {
                prpExist = PrpMgr.Get6(Name, UseCached, out val, out resValStr, out _, out _) != (int)swCustomInfoGetResult_e.swCustomInfoGetResult_NotPresent;
            }
            else if (m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2014))
            {
                prpExist = PrpMgr.Get5(Name, UseCached, out val, out resValStr, out _) != (int)swCustomInfoGetResult_e.swCustomInfoGetResult_NotPresent;
            }
            else
            {
                prpExist = PrpMgr.Get4(Name, UseCached, out val, out resValStr);
            }

            if (prpExist)
            {
                resVal = null;

                switch ((swCustomInfoType_e)PrpMgr.GetType2(Name))
                {
                    case swCustomInfoType_e.swCustomInfoText:
                        resVal = resValStr;
                        break;

                    case swCustomInfoType_e.swCustomInfoYesOrNo:
                        switch (resValStr.ToLower()) 
                        {
                            case "yes":
                                resVal = true;
                                break;

                            case "no":
                                resVal = false;
                                break;

                            default:
                                if (bool.TryParse(resValStr, out var boolVal))
                                {
                                    resVal = boolVal;
                                }
                                else 
                                {
                                    resVal = resValStr;
                                }
                                break;
                        }
                        break;

                    case swCustomInfoType_e.swCustomInfoDouble:
                    case swCustomInfoType_e.swCustomInfoNumber:
                        resVal = double.Parse(resValStr);
                        break;

                    case swCustomInfoType_e.swCustomInfoDate:
                        resVal = DateTime.Parse(resValStr);
                        break;
                }
            }
            else
            {
                resVal = null;
            }
        }

        public override void Commit(CancellationToken cancellationToken)
        {
            if (!IsCommitted)
            {
                AddProperty(PrpMgr, Name, Value);

                m_IsCommitted = true;
            }
            else 
            {
                throw new Exception("Property already committed");
            }
        }

        protected virtual void AddProperty(ICustomPropertyManager prpMgr, string name, object value)
        {
            if (string.IsNullOrEmpty(name)) 
            {
                throw new Exception("Custom property name is not specified");
            }

            if (m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2014))
            {
                var res = (swCustomInfoAddResult_e)prpMgr.Add3(name, (int)swCustomInfoType_e.swCustomInfoText, 
                    value?.ToString(), (int)swCustomPropertyAddOption_e.swCustomPropertyOnlyIfNew);

                if (res != swCustomInfoAddResult_e.swCustomInfoAddResult_AddedOrChanged)
                {
                    throw new Exception($"Failed to add {Name}. Error code: {res}");
                }
            }
            else 
            {
                const int SUCCESS = 1;

                if (prpMgr.Add2(name, (int)swCustomInfoType_e.swCustomInfoText, value?.ToString()) != SUCCESS)
                {
                    throw new Exception($"Failed to add {Name}");
                }
            }
        }

        protected virtual void SetProperty(ICustomPropertyManager prpMgr, string name, object value) 
        {
            if (m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2014))
            {
                var res = (swCustomInfoSetResult_e)prpMgr.Set2(name, value?.ToString());

                if (res != swCustomInfoSetResult_e.swCustomInfoSetResult_OK)
                {
                    throw new Exception($"Failed to set the value of the property '{name}'. Error code: {res}");
                }
            }
            else 
            {
                const int SUCCESS = 0;

                if (prpMgr.Set(name, value?.ToString()) != SUCCESS) 
                {
                    throw new Exception($"Failed to set the value of the property '{name}'");
                }
            }
        }
    }

    internal class SwConfigurationCustomProperty : SwCustomProperty
    {
        private readonly SwConfiguration m_Conf;
        
        internal SwConfigurationCustomProperty(Func<ICustomPropertyManager> prpMgrFact, string name, bool isCommited, SwConfiguration conf, SwApplication app)
            : base(prpMgrFact, name, isCommited, conf.OwnerDocument, app)
        {
            m_Conf = conf;
        }

        protected override void AddProperty(ICustomPropertyManager prpMgr, string name, object value)
        {
            base.AddProperty(prpMgr, name, value);

            if (m_App.Version.Major == SwVersion_e.Sw2021 && !m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2021, 4, 1)) //regression bug in SW 2021 where property cannot be added to unloaded configuration
            {
                if (!m_Conf.Configuration.IsLoaded())
                {
                    if (!((string[])prpMgr.GetNames() ?? new string[0]).Contains(name, StringComparer.CurrentCultureIgnoreCase))
                    {
                        throw new CustomPropertyUnloadedConfigException();
                    }
                }
            }
        }
    }
}
