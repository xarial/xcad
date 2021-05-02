//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Delegates;
using Xarial.XCad.SolidWorks.Data.EventHandlers;
using Xarial.XCad.SolidWorks.Data.Helpers;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.SolidWorks.Data
{
    public interface ISwCustomProperty : IXProperty
    {
    }

    internal class SwCustomProperty : ISwCustomProperty
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

        protected virtual ICustomPropertyManager PrpMgr { get; }
        
        public bool IsCommitted 
        {
            get;
            private set;
        }

        private readonly ISwApplication m_App;

        internal SwCustomProperty(CustomPropertyManager prpMgr, string name, bool isCommited, ISwApplication app)
        {
            PrpMgr = prpMgr;
            m_Name = name;
            IsCommitted = isCommited;
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

            var useCached = true;

            if (m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2018))
            {
                prpExist = PrpMgr.Get6(Name, useCached, out val, out resValStr, out _, out _) != (int)swCustomInfoGetResult_e.swCustomInfoGetResult_NotPresent;
            }
            else if (m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2014))
            {
                prpExist = PrpMgr.Get5(Name, useCached, out val, out resValStr, out _) != (int)swCustomInfoGetResult_e.swCustomInfoGetResult_NotPresent;
            }
            else
            {
                prpExist = PrpMgr.Get4(Name, useCached, out val, out resValStr);
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
                        resVal = bool.Parse(resValStr);
                        break;

                    case swCustomInfoType_e.swCustomInfoDouble:
                        resVal = double.Parse(resValStr);
                        break;

                    case swCustomInfoType_e.swCustomInfoNumber:
                        resVal = int.Parse(resValStr);
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

        public void Commit(CancellationToken cancellationToken)
        {
            if (!IsCommitted)
            {
                AddProperty(PrpMgr, Name, Value);

                IsCommitted = true;
            }
            else 
            {
                throw new Exception("Property already committed");
            }
        }

        protected virtual void AddProperty(ICustomPropertyManager prpMgr, string name, object value)
        {
            const int SUCCESS = 1;

            //TODO: fix type conversion
            if (prpMgr.Add2(name, (int)swCustomInfoType_e.swCustomInfoText, value?.ToString()) != SUCCESS)
            {
                throw new Exception($"Failed to add {Name}");
            }
        }

        protected virtual void SetProperty(ICustomPropertyManager prpMgr, string name, object value) 
        {
            var res = (swCustomInfoSetResult_e)prpMgr.Set2(name, value?.ToString());

            if (res != swCustomInfoSetResult_e.swCustomInfoSetResult_OK)
            {
                throw new Exception($"Failed to set the value of the property. Error code: {res}");
            }
        }
    }
}
