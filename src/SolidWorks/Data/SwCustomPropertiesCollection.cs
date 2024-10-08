//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
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

    internal class PropertyEntityCache : EntityCache<IXProperty>
    {
        private readonly IPropertiesOwner m_Owner;

        public PropertyEntityCache(IPropertiesOwner owner, IXRepository<IXProperty> repo, Func<IXProperty, string> nameProvider) 
            : base(owner, repo, nameProvider)
        {
            m_Owner = owner;
        }

        protected override void CommitEntitiesFromCache(IReadOnlyList<IXProperty> ents, CancellationToken cancellationToken)
        {
            foreach (var ent in ents)
            {
                //NOTE: when new configuration is created it may copy proepties from the source configuration
                //and it is required to overwrite their values instead of adding new as it may fail
                if (m_Owner.Properties.TryGet(ent.Name, out var prp))
                {
                    prp.Value = ent.Value;
                }
                else 
                {
                    ent.Commit(cancellationToken);
                }
            }
        }
    }

    internal abstract class SwCustomPropertiesCollection : ISwCustomPropertiesCollection
    {
        IXProperty IXRepository<IXProperty>.this[string name] => this[name];
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public ISwCustomProperty this[string name]
        {
            get
            {
                try
                {
                    return (SwCustomProperty)m_RepoHelper.Get(name);
                }
                catch (EntityNotFoundException)
                {
                    throw new CustomPropertyMissingException(name);
                }
            }
        }

        public bool TryGet(string name, out IXProperty ent)
        {
            if (m_Owner.IsCommitted)
            {
                if (Exists(name))
                {
                    ent = CreatePropertyInstance(name, true);
                    return true;
                }
                else
                {
                    ent = null;
                    return false;
                }
            }
            else 
            {
                return m_Cache.TryGet(name, out ent);
            }
        }

        public int Count
        {
            get
            {
                if (m_Owner.IsCommitted)
                {
                    return PrpMgr.Count;
                }
                else 
                {
                    return m_Cache.Count;
                }
            }
        }

        protected abstract CustomPropertyManager PrpMgr { get; }

        protected readonly ISwApplication m_App;

        private readonly RepositoryHelper<IXProperty> m_RepoHelper;

        private readonly IPropertiesOwner m_Owner;

        private readonly EntityCache<IXProperty> m_Cache;

        protected SwCustomPropertiesCollection(IPropertiesOwner owner, ISwApplication app)
        {
            m_App = app;
            m_Owner = owner;

            m_RepoHelper = new RepositoryHelper<IXProperty>(this,
                TransactionFactory<IXProperty>.Create(() => PreCreate()));

            m_Cache = new PropertyEntityCache(owner, this, c => c.Name);
        }

        public T PreCreate<T>() where T : IXProperty => m_RepoHelper.PreCreate<T>();

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

        public void AddRange(IEnumerable<IXProperty> ents, CancellationToken cancellationToken)
        {
            if (m_Owner.IsCommitted)
            {
                m_RepoHelper.AddRange(ents, cancellationToken);
            }
            else 
            {
                m_Cache.AddRange(ents, cancellationToken);
            }
        }

        public IEnumerator<IXProperty> GetEnumerator()
        {
            if (m_Owner.IsCommitted)
            {
                return IterateProperties().GetEnumerator();
            }
            else 
            {
                return m_Cache.GetEnumerator();
            }
        }

        public void RemoveRange(IEnumerable<IXProperty> ents, CancellationToken cancellationToken)
        {
            if (m_Owner.IsCommitted)
            {
                foreach (var prp in ents)
                {
                    DeleteProperty(prp);
                }
            }
            else
            {
                m_Cache.RemoveRange(ents, cancellationToken);
            }
        }

        internal void CommitCache(CancellationToken cancellationToken) => m_Cache.Commit(cancellationToken);

        protected virtual void DeleteProperty(IXProperty prp)
        {
            if (m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2014))
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

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public ISwCustomProperty PreCreate() => CreatePropertyInstance("", false);

        private IEnumerable<SwCustomProperty> IterateProperties() 
        {
            var prpNames = (string[])PrpMgr.GetNames() ?? Array.Empty<string>();

            foreach(var prpName in prpNames) 
            {
                if (!string.Equals(prpName, SwConfiguration.QTY_PROPERTY, StringComparison.CurrentCultureIgnoreCase))
                {
                    yield return CreatePropertyInstance(prpName, true);
                }
            }
        }

        protected void InitProperty(SwCustomProperty prp)
        {
            prp.SetEventsHandler(CreateEventsHandler(prp));
        }

        protected abstract EventsHandler<PropertyValueChangedDelegate> CreateEventsHandler(SwCustomProperty prp);
        protected abstract SwCustomProperty CreatePropertyInstance(string name, bool isCreated);
        
        public virtual void Dispose()
        {
        }
    }

    internal class SwFileCustomPropertiesCollection : SwCustomPropertiesCollection
    {
        private readonly CustomPropertiesEventsHelper m_EventsHelper;
        private readonly List<EventsHandler<PropertyValueChangedDelegate>> m_EventsHandlers;

        private readonly SwDocument m_Doc;

        internal SwFileCustomPropertiesCollection(SwDocument doc, ISwApplication app) : this(doc, doc, app)
        {
        }

        protected SwFileCustomPropertiesCollection(SwDocument doc, IPropertiesOwner owner, ISwApplication app) : base(owner, app)
        {
            m_Doc = doc;
            m_EventsHelper = new CustomPropertiesEventsHelper(app.Sw, doc);

            m_EventsHandlers = new List<EventsHandler<PropertyValueChangedDelegate>>();
        }

        protected override CustomPropertyManager PrpMgr => m_Doc.Model.Extension.CustomPropertyManager[GetConfiguration()?.Name ?? ""];

        protected override SwCustomProperty CreatePropertyInstance(string name, bool isCreated)
        {
            var prp = new SwCustomProperty(() => PrpMgr, name, isCreated, m_App);
            InitProperty(prp);
            return prp;
        }

        protected override EventsHandler<PropertyValueChangedDelegate> CreateEventsHandler(SwCustomProperty prp)
        {
            EventsHandler<PropertyValueChangedDelegate> evHandler;

            if (m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2017) && !m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2020, 5))
            {
                evHandler = new CustomPropertyChangeEventsHandlerFromSw2017(m_EventsHelper, m_Doc, prp, GetConfiguration());
            }
            else
            {
                evHandler = new CustomPropertyChangeEventsHandler(m_Doc, prp, GetConfiguration());
            }

            m_EventsHandlers.Add(evHandler);

            return evHandler;
        }

        protected virtual SwConfiguration GetConfiguration() => null;

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

    internal class SwConfigurationCustomPropertiesCollection : SwFileCustomPropertiesCollection
    {
        private readonly SwConfiguration m_Conf;

        internal SwConfigurationCustomPropertiesCollection(SwConfiguration conf, SwDocument doc, ISwApplication app)
            : base(doc, conf, app)
        {
            m_Conf = conf;
        }

        protected override SwCustomProperty CreatePropertyInstance(string name, bool isCreated)
        {
            var prp = new SwConfigurationCustomProperty(() => PrpMgr, name, isCreated, m_Conf, m_App);
            InitProperty(prp);
            return prp;
        }

        protected override SwConfiguration GetConfiguration() => m_Conf;
    }
}
