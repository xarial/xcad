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
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.SolidWorks.Data
{
    public interface ISwCustomProperty : IXProperty
    {
        string Expression { get; set; }
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
                    var res = (swCustomInfoSetResult_e)m_PrpMgr.Set2(Name, value?.ToString());

                    if (res != swCustomInfoSetResult_e.swCustomInfoSetResult_OK)
                    {
                        throw new Exception($"Failed to set the value of the property. Error code: {res}");
                    }
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
                Value = value;
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

        private readonly ICustomPropertyManager m_PrpMgr;
        
        public bool IsCommitted 
        {
            get;
            private set;
        }

        internal SwCustomProperty(CustomPropertyManager prpMgr, string name, bool isCommited)
        {
            m_PrpMgr = prpMgr;
            m_Name = name;
            IsCommitted = isCommited;
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
            
            if (m_PrpMgr.Get4(Name, false, out val, out resValStr))
            {
                resVal = null;

                switch ((swCustomInfoType_e)m_PrpMgr.GetType2(Name))
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
                const int SUCCESS = 1;

                //TODO: fix type conversion
                if (m_PrpMgr.Add2(Name, (int)swCustomInfoType_e.swCustomInfoText, Value.ToString()) != SUCCESS)
                {
                    throw new Exception($"Failed to add {Name}");
                }

                IsCommitted = true;
            }
            else 
            {
                throw new Exception("Property already committed");
            }
        }
    }
}
