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
using Xarial.XCad.Data;
using Xarial.XCad.Data.Delegates;
using Xarial.XCad.SolidWorks.Data.EventHandlers;
using Xarial.XCad.SolidWorks.Data.Helpers;

namespace Xarial.XCad.SolidWorks.Data
{
    public class SwCustomProperty : IXProperty
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
                if (TryReadProperty(out _, out object resVal))
                {
                    return resVal;
                }
                else 
                {
                    return m_TempValue;
                }
            }
            set
            {
                if (Exists)
                {
                    var res = (swCustomInfoSetResult_e)m_PrpMgr.Set2(Name, value.ToString());

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
                if (TryReadProperty(out string val, out _))
                {
                    return val;
                }
                else
                {
                    return null;
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

        private readonly CustomPropertyChangeEventsHandler m_CustomPropertyChangeEventsHandler;

        private IModelDoc2 m_Model;
        private ICustomPropertyManager m_PrpMgr;

        public string ConfigurationName { get; }

        public bool Exists => TryReadProperty(out _, out _);

        internal SwCustomProperty(IModelDoc2 model, ICustomPropertyManager prpMgr, string name, 
            string confName, CustomPropertiesEventsHelper evHelper) 
        {
            m_Model = model;
            m_PrpMgr = prpMgr;
            m_Name = name;
            ConfigurationName = confName;

            var isBugPresent = true; //TODO: find version when the issue is starter

            if (isBugPresent)
            {
                m_CustomPropertyChangeEventsHandler = new CustomPropertyChangeEventsHandlerFromSw2017(evHelper, model, this);
            }
            else 
            {
                m_CustomPropertyChangeEventsHandler = new CustomPropertyChangeEventsHandler(model, this);
            }
        }

        private void RenameProperty(string newName) 
        {
            m_Name = newName;

            if (Exists)
            {
                throw new NotImplementedException();
            }
        }

        private bool TryReadProperty(out string val, out object resVal)
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

                return true;
            }
            else
            {
                resVal = null;
                return false;
            }
        }
    }
}
