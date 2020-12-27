using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Delegates;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Data;
using Xarial.XCad.SolidWorks.Data.EventHandlers;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.SolidWorks.Features
{
    public interface ISwCutListItem : IXCutListItem, ISwFeature, IDisposable
    {
        IBodyFolder CutListBodyFolder { get; }
    }

    internal class SafeFeatureProvider
    {
        private Dictionary<SwCutListItem, IFeature> m_FeaturesMap;
        private Dictionary<int, SwCutListItem> m_FeatureIdsMap;

        private readonly SwDocument3D m_Doc;
        private readonly SwConfiguration m_Conf;

        internal SafeFeatureProvider(SwCutListItem[] cutLists, SwDocument3D doc, SwConfiguration conf) 
        {
            m_FeaturesMap = cutLists.ToDictionary(x => x, x => x.Feature);
            m_FeatureIdsMap = cutLists.ToDictionary(x => x.Feature.GetID(), x => x);
            m_Doc = doc;
            m_Conf = conf;
        }

        internal IFeature ProvideFeature(SwCutListItem cutListItem) 
        {
            var feat = m_FeaturesMap[cutListItem];

            if (!IsFeatureAlive(feat)/* || m_Conf.Configuration.IsDirty()*/)
            {
                RelinkAllFeatures();
                feat = m_FeaturesMap[cutListItem];
            }

            return feat;
        }

        private void RelinkAllFeatures()
        {
            var activeConf = m_Doc.Configurations.Active;

            m_Doc.Configurations.Active = m_Conf;

            foreach (ISwFeature feat in m_Doc.Features)
            {
                if (m_FeatureIdsMap.TryGetValue(feat.Feature.GetID(), out SwCutListItem cutListItem))
                {
                    m_FeaturesMap[cutListItem] = feat.Feature;
                }

                if (feat.Feature.GetTypeName2() == "RefPlane")
                {
                    break;
                }
            }

            m_Doc.Configurations.Active = activeConf;
        }

        private bool IsFeatureAlive(IFeature feat) 
        {
            try
            {
                var name = feat.Name;
                return true;
            }
            catch 
            {
                return false;
            }
        }
    }

    internal class SwCutListItem : SwFeature, ISwCutListItem
    {
        private readonly Lazy<ISwCustomPropertiesCollection> m_Properties;

        IXPropertyRepository IPropertiesOwner.Properties => Properties;

        internal SwCutListItem(ISwDocument doc, IFeature feat, bool created) : base(doc, feat, created)
        {
            if (feat.GetTypeName2() != "CutListFolder") 
            {
                throw new InvalidCastException("Specified feature is not a cut-list feature");
            }

            CutListBodyFolder = (IBodyFolder)feat.GetSpecificFeature2();

            m_Properties = new Lazy<ISwCustomPropertiesCollection>(
                () => CreatePropertiesCollection());
        }

        private SafeFeatureProvider m_SafeFeatureProvider;

        public override IFeature Feature
        {
            get 
            {
                if (m_SafeFeatureProvider != null)
                {
                    return m_SafeFeatureProvider.ProvideFeature(this);
                }
                else
                {
                    return base.Feature;
                }
            }
        }

        internal void SetSafeFeatureProvider(SafeFeatureProvider provider) => m_SafeFeatureProvider = provider;

        protected virtual SwCutListCustomPropertiesCollection CreatePropertiesCollection()
            => new SwCutListCustomPropertiesCollection(m_Doc, () => m_SafeFeatureProvider.ProvideFeature(this));

        public IBodyFolder CutListBodyFolder { get; }

        public IXSolidBody[] Bodies 
        {
            get
            {
                var bodies = CutListBodyFolder.GetBodies() as object[];

                if (bodies != null)
                {
                    return bodies.Select(b => SwObjectFactory.FromDispatch<ISwSolidBody>(b, m_Doc)).ToArray();
                }
                else 
                {
                    return new IXSolidBody[0];
                }
            }
        }

        public ISwCustomPropertiesCollection Properties => m_Properties.Value;

        public void Dispose()
        {
            if (m_Properties.IsValueCreated) 
            {
                m_Properties.Value.Dispose();
            }
        }
    }

    internal class SwCutListCustomPropertiesCollection : SwCustomPropertiesCollection
    {
        private readonly Func<IFeature> m_FeatProvider;

        internal SwCutListCustomPropertiesCollection(ISwDocument doc, Func<IFeature> featProvider) 
            : base((SwDocument)doc)
        {
            m_FeatProvider = featProvider;
        }

        protected override CustomPropertyManager PrpMgr 
            => m_FeatProvider.Invoke().CustomPropertyManager;

        protected override EventsHandler<PropertyValueChangedDelegate> CreateEventsHandler(SwCustomProperty prp)
            => new CutListCustomPropertyChangeEventsHandler();

        protected override SwCustomProperty CreatePropertyInstance(CustomPropertyManager prpMgr, string name, bool isCreated)
            => new SwCutListCustomProperty(m_FeatProvider, name, isCreated);
    }

    internal class SwCutListCustomProperty : SwCustomProperty
    {
        private readonly Func<IFeature> m_FeatProvider;

        internal SwCutListCustomProperty(Func<IFeature> featProvider, string name, bool isCommited) 
            : base(featProvider.Invoke().CustomPropertyManager, name, isCommited)
        {
            m_FeatProvider = featProvider;
        }

        protected override ICustomPropertyManager PrpMgr => m_FeatProvider.Invoke().CustomPropertyManager;
    }

    public class CutListCustomPropertyChangeEventsHandler : EventsHandler<PropertyValueChangedDelegate>
    {
        protected override void SubscribeEvents()
        {
            throw new NotImplementedException();
        }

        protected override void UnsubscribeEvents()
        {
            throw new NotImplementedException();
        }
    }
}
