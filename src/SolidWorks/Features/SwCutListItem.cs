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

        protected virtual SwCutListCustomPropertiesCollection CreatePropertiesCollection()
            => new SwCutListCustomPropertiesCollection(m_Doc, Feature.CustomPropertyManager);

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
        internal SwCutListCustomPropertiesCollection(ISwDocument doc, CustomPropertyManager prpsMgr) 
            : base((SwDocument)doc)
        {
            PrpMgr = prpsMgr;
        }

        protected override CustomPropertyManager PrpMgr { get; }

        protected override EventsHandler<PropertyValueChangedDelegate> CreateEventsHandler(SwCustomProperty prp)
            => new CutListCustomPropertyChangeEventsHandler();
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
