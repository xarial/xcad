//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Exceptions;
using Xarial.XCad.SolidWorks.Data.Exceptions;
using Xarial.XCad.SolidWorks.Data.Helpers;

namespace Xarial.XCad.SolidWorks.Data
{
    public class SwCustomPropertiesCollection : IXPropertyRepository, IDisposable
    {
        IXProperty IXPropertyRepository.GetOrPreCreate(string name) => GetOrPreCreate(name);

        IXProperty IXRepository<IXProperty>.this[string name] => this[name];

        public SwCustomProperty this[string name] 
        {
            get 
            {
                try
                {
                    return (SwCustomProperty)this.Get(name);
                }
                catch (EntityNotFoundException) 
                {
                    throw new CustomPropertyMissingException(name);
                }
            }
        }

        public bool TryGet(string name, out IXProperty ent)
        {
            var prp = GetOrPreCreate(name);

            if (prp.Exists)
            {
                ent = prp;
                return true;
            }
            else
            {
                ent = null;
                return false;
            }
        }

        public int Count => m_PrpMgr.Count;

        private readonly IModelDoc2 m_Model;
        private readonly ICustomPropertyManager m_PrpMgr;

        private readonly string m_ConfName;

        private readonly CustomPropertiesEventsHelper m_EventsHelper;

        internal SwCustomPropertiesCollection(ISldWorks app, IModelDoc2 model, string confName) 
        {
            m_Model = model;
            m_PrpMgr = model.Extension.CustomPropertyManager[confName];

            m_EventsHelper = new CustomPropertiesEventsHelper(app, model);

            m_ConfName = confName;
        }

        public void AddRange(IEnumerable<IXProperty> ents)
        {
            const int SUCCESS = 1;

            foreach (var prp in ents) 
            {
                //TODO: fix type conversion
                if (m_PrpMgr.Add2(prp.Name, (int)swCustomInfoType_e.swCustomInfoText, prp.Value.ToString()) != SUCCESS)
                {
                    throw new Exception($"Failed to add {prp.Name}");
                }
            }
        }

        public IEnumerator<IXProperty> GetEnumerator()
        {
            return new SwCustomPropertyEnumerator(m_Model, m_PrpMgr, m_ConfName, m_EventsHelper);
        }

        public void RemoveRange(IEnumerable<IXProperty> ents)
        {
            const int SUCCESS = 0;

            foreach (var prp in ents)
            {
                //TODO: fix the versions
                if (m_PrpMgr.Delete(prp.Name) != SUCCESS)
                {
                    throw new Exception($"Failed to remove {prp.Name}");
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public SwCustomProperty GetOrPreCreate(string name)
        {
            return new SwCustomProperty(m_Model, m_PrpMgr, name, m_ConfName, m_EventsHelper);
        }

        public void Dispose()
        {
            m_EventsHelper.Dispose();
        }
    }

    internal class SwCustomPropertyEnumerator : IEnumerator<IXProperty>
    {
        public IXProperty Current => new SwCustomProperty(m_Model, m_PrpMgr, m_PrpNames[m_CurPrpIndex], m_ConfName, m_EvHelper);

        object IEnumerator.Current => Current;

        private readonly CustomPropertiesEventsHelper m_EvHelper;

        private readonly IModelDoc2 m_Model;
        private readonly ICustomPropertyManager m_PrpMgr;

        private readonly string m_ConfName;
        private readonly string[] m_PrpNames;
        private int m_CurPrpIndex;

        internal SwCustomPropertyEnumerator(IModelDoc2 model, ICustomPropertyManager prpMgr, string confName, CustomPropertiesEventsHelper evHelper) 
        {
            m_Model = model;
            m_PrpMgr = prpMgr;
            m_ConfName = confName;

            m_EvHelper = evHelper;

            m_PrpNames = m_PrpMgr.GetNames() as string[];
            
            if (m_PrpNames == null) 
            {
                m_PrpNames = new string[0];
            }

            m_CurPrpIndex = -1;
        }

        public bool MoveNext()
        {
            m_CurPrpIndex++;

            return m_CurPrpIndex < m_PrpNames.Length;
        }

        public void Reset()
        {
            m_CurPrpIndex = -1;
        }

        public void Dispose()
        {
        }
    }
}
