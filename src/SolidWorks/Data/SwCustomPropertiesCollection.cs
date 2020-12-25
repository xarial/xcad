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
using Xarial.XCad.Data.Delegates;
using Xarial.XCad.Exceptions;
using Xarial.XCad.SolidWorks.Data.EventHandlers;
using Xarial.XCad.SolidWorks.Data.Exceptions;
using Xarial.XCad.SolidWorks.Data.Helpers;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.SolidWorks.Data
{
    public interface ISwCustomPropertiesCollection : IXPropertyRepository, IDisposable
    {
        new ISwCustomProperty this[string name] { get; }
        new ISwCustomProperty GetOrPreCreate(string name);
    }

    internal abstract class SwCustomPropertiesCollection : ISwCustomPropertiesCollection
    {
        IXProperty IXPropertyRepository.GetOrPreCreate(string name) => GetOrPreCreate(name);

        IXProperty IXRepository<IXProperty>.this[string name] => this[name];

        public ISwCustomProperty this[string name]
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

            if (prp.IsCommitted)
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

        public int Count => PrpMgr.Count;

        protected IModelDoc2 Model => m_Doc.Model;

        protected abstract CustomPropertyManager PrpMgr { get; }

        protected ISwDocument m_Doc;

        protected SwCustomPropertiesCollection(ISwDocument doc)
        {
            m_Doc = doc;
        }

        public void AddRange(IEnumerable<IXProperty> ents)
        {
            foreach (var prp in ents)
            {
                prp.Commit();
            }
        }

        public IEnumerator<IXProperty> GetEnumerator()
            => new SwCustomPropertyEnumerator(PrpMgr, CreateEventsHandler);

        public void RemoveRange(IEnumerable<IXProperty> ents)
        {
            const int SUCCESS = 0;

            foreach (var prp in ents)
            {
                //TODO: fix the versions
                if (PrpMgr.Delete(prp.Name) != SUCCESS)
                {
                    throw new Exception($"Failed to remove {prp.Name}");
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected abstract EventsHandler<PropertyValueChangedDelegate> CreateEventsHandler(SwCustomProperty prp);

        public ISwCustomProperty GetOrPreCreate(string name)
        {
            var prp = new SwCustomProperty(PrpMgr, name);
            prp.SetEventsHandler(CreateEventsHandler(prp));

            return prp;
        }

        public virtual void Dispose()
        {
        }
    }

    internal class SwConfigurationCustomPropertiesCollection : SwCustomPropertiesCollection
    {
        private readonly string m_ConfName;

        private readonly CustomPropertiesEventsHelper m_EventsHelper;
        private readonly List<EventsHandler<PropertyValueChangedDelegate>> m_EventsHandlers;

        internal SwConfigurationCustomPropertiesCollection(SwDocument doc, string confName) : base(doc)
        {
            m_EventsHelper = new CustomPropertiesEventsHelper(doc.App.Sw, doc);

            m_ConfName = confName;

            m_EventsHandlers = new List<EventsHandler<PropertyValueChangedDelegate>>();
        }

        protected override CustomPropertyManager PrpMgr => Model.Extension.CustomPropertyManager[m_ConfName];
                
        protected override EventsHandler<PropertyValueChangedDelegate> CreateEventsHandler(SwCustomProperty prp)
        {
            var isBugPresent = true; //TODO: find version when the issue is starter

            EventsHandler<PropertyValueChangedDelegate> evHandler = null;

            if (isBugPresent)
            {
                evHandler = new CustomPropertyChangeEventsHandlerFromSw2017(m_EventsHelper, m_Doc.Model, prp, m_ConfName);
            }
            else
            {
                evHandler = new CustomPropertyChangeEventsHandler(m_Doc.Model, prp, m_ConfName);
            }

            m_EventsHandlers.Add(evHandler);

            return evHandler;
        }

        public override void Dispose()
        {
            base.Dispose();

            m_EventsHelper.Dispose();

            foreach (var evHandler in m_EventsHandlers) 
            {
                evHandler.Dispose();
            }
        }
    }

    internal class SwFileCustomPropertiesCollection : SwConfigurationCustomPropertiesCollection
    {
        internal SwFileCustomPropertiesCollection(SwDocument doc) : base(doc, "")
        {
        }
    }

    internal class SwCustomPropertyEnumerator : IEnumerator<IXProperty>
    {
        public IXProperty Current
        {
            get
            {
                var prp = new SwCustomProperty(m_PrpMgr, m_PrpNames[m_CurPrpIndex]);
                prp.SetEventsHandler(m_EventsHandlerFact.Invoke(prp));
                return prp;
            }
        }
            
        object IEnumerator.Current => Current;

        private readonly CustomPropertyManager m_PrpMgr;

        private readonly string[] m_PrpNames;
        private int m_CurPrpIndex;

        private readonly Func<SwCustomProperty, EventsHandler<PropertyValueChangedDelegate>> m_EventsHandlerFact;

        internal SwCustomPropertyEnumerator(CustomPropertyManager prpMgr, 
            Func<SwCustomProperty, EventsHandler<PropertyValueChangedDelegate>> eventsHandlerFact) 
        {
            m_PrpMgr = prpMgr;

            m_EventsHandlerFact = eventsHandlerFact;

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
