//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Delegates;
using Xarial.XCad.Exceptions;
using Xarial.XCad.SolidWorks.Data.EventHandlers;
using Xarial.XCad.SolidWorks.Data.Exceptions;
using Xarial.XCad.SolidWorks.Data.Helpers;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Data
{
    public interface ISwCustomPropertiesCollection : IXPropertyRepository, IDisposable
    {
        new ISwCustomProperty this[string name] { get; }
        ISwCustomProperty PreCreate();
    }

    internal abstract class SwCustomPropertiesCollection : ISwCustomPropertiesCollection
    {
        IXProperty IXRepository<IXProperty>.this[string name] => this[name];

        public ISwCustomProperty this[string name]
        {
            get
            {
                try
                {
                    return (SwCustomProperty)RepositoryHelper.Get(this, name);
                }
                catch (EntityNotFoundException)
                {
                    throw new CustomPropertyMissingException(name);
                }
            }
        }

        public bool TryGet(string name, out IXProperty ent)
        {
            if (Exists(name))
            {
                ent = CreatePropertyInstance(PrpMgr, name, true);
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

        protected SwDocument m_Doc;

        protected readonly ISwApplication m_App;

        protected SwCustomPropertiesCollection(SwDocument doc, ISwApplication app)
        {
            m_Doc = doc;
            m_App = app;
        }

        private bool Exists(string name) 
        {
            if (m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2018))
            {
                return PrpMgr.Get6(name, true, out _, out _, out _, out _) != (int)swCustomInfoGetResult_e.swCustomInfoGetResult_NotPresent;
            }
            else if (m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2014))
            {
                return PrpMgr.Get5(name, true, out _, out _, out _) != (int)swCustomInfoGetResult_e.swCustomInfoGetResult_NotPresent;
            }
            else 
            {
                var prpNames = PrpMgr.GetNames() as string[] ?? new string[0];
                return prpNames.Contains(name, StringComparer.CurrentCultureIgnoreCase);
            }
        }

        public void AddRange(IEnumerable<IXProperty> ents, CancellationToken cancellationToken) => RepositoryHelper.AddRange(ents, cancellationToken);

        public IEnumerator<IXProperty> GetEnumerator()
            => new SwCustomPropertyEnumerator(PrpMgr, CreatePropertyInstance);

        public void RemoveRange(IEnumerable<IXProperty> ents, CancellationToken cancellationToken)
        {
            foreach (var prp in ents)
            {
                DeleteProperty(prp);
            }
        }

        protected virtual void DeleteProperty(IXProperty prp)
        {
            if (m_App.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2014))
            {
                var delRes = (swCustomInfoDeleteResult_e)PrpMgr.Delete2(prp.Name);

                if (delRes != swCustomInfoDeleteResult_e.swCustomInfoDeleteResult_OK)
                {
                    throw new Exception($"Failed to remove property '{prp.Name}'. Error code: {delRes}");
                }
            }
            else
            {
                const int SUCCESS = 0;

                if (PrpMgr.Delete(prp.Name) != SUCCESS)
                {
                    throw new Exception($"Failed to remove property '{prp.Name}'");
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) => RepositoryHelper.FilterDefault(this, filters, reverseOrder);

        protected abstract EventsHandler<PropertyValueChangedDelegate> CreateEventsHandler(SwCustomProperty prp);

        public ISwCustomProperty PreCreate() => CreatePropertyInstance(PrpMgr, "", false);

        protected abstract SwCustomProperty CreatePropertyInstance(CustomPropertyManager prpMgr, string name, bool isCreated);

        protected void InitProperty(SwCustomProperty prp)
        {
            prp.SetEventsHandler(CreateEventsHandler(prp));
        }

        public virtual void Dispose()
        {
        }

        public T PreCreate<T>() where T : IXProperty => (T)PreCreate();
    }

    internal class SwConfigurationCustomPropertiesCollection : SwCustomPropertiesCollection
    {
        private readonly string m_ConfName;

        private readonly CustomPropertiesEventsHelper m_EventsHelper;
        private readonly List<EventsHandler<PropertyValueChangedDelegate>> m_EventsHandlers;

        internal SwConfigurationCustomPropertiesCollection(string confName, SwDocument doc, ISwApplication app) : base(doc, app)
        {
            m_EventsHelper = new CustomPropertiesEventsHelper(app.Sw, doc);

            m_ConfName = confName;

            m_EventsHandlers = new List<EventsHandler<PropertyValueChangedDelegate>>();
        }

        protected override CustomPropertyManager PrpMgr => Model.Extension.CustomPropertyManager[m_ConfName];

        protected override SwCustomProperty CreatePropertyInstance(CustomPropertyManager prpMgr, string name, bool isCreated)
        {
            var prp = new SwConfigurationCustomProperty(prpMgr, name, isCreated, m_Doc, m_ConfName, m_App);
            InitProperty(prp);
            return prp;
        }

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
        internal SwFileCustomPropertiesCollection(SwDocument doc, ISwApplication app) : base("", doc, app)
        {
        }

        protected override SwCustomProperty CreatePropertyInstance(CustomPropertyManager prpMgr, string name, bool isCreated)
        {
            var prp = new SwCustomProperty(prpMgr, name, isCreated, m_App);
            InitProperty(prp);
            return prp;
        }
    }

    internal class SwCustomPropertyEnumerator : IEnumerator<IXProperty>
    {
        public IXProperty Current => m_PrpFact.Invoke(m_PrpMgr, m_PrpNames[m_CurPrpIndex], true);
        
        object IEnumerator.Current => Current;

        protected readonly CustomPropertyManager m_PrpMgr;

        private readonly string[] m_PrpNames;
        private int m_CurPrpIndex;

        private readonly Func<CustomPropertyManager, string, bool, SwCustomProperty> m_PrpFact;

        internal SwCustomPropertyEnumerator(CustomPropertyManager prpMgr, 
            Func<CustomPropertyManager, string, bool, SwCustomProperty> prpFact) 
        {
            m_PrpMgr = prpMgr;
            m_PrpFact = prpFact;

            m_PrpNames = m_PrpMgr.GetNames() as string[];
            
            if (m_PrpNames == null) 
            {
                m_PrpNames = new string[0];
            }

            m_PrpNames = m_PrpNames.Except(new string[] { SwConfiguration.QTY_PROPERTY }).ToArray();

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
