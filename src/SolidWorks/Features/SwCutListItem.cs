//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Delegates;
using Xarial.XCad.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Data;
using Xarial.XCad.SolidWorks.Data.EventHandlers;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Documents.Exceptions;
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

        private ISwDocument3D m_OwnerDoc;
        private ISwConfiguration m_OwnerConf;

        internal SwCutListItem(ISwDocument3D doc, IFeature feat, bool created) : base(doc, feat, created)
        {
            if (feat.GetTypeName2() != "CutListFolder") 
            {
                throw new InvalidCastException("Specified feature is not a cut-list feature");
            }

            m_OwnerDoc = doc;
            m_OwnerConf = doc.Configurations.Active;

            CutListBodyFolder = (IBodyFolder)feat.GetSpecificFeature2();

            m_Properties = new Lazy<ISwCustomPropertiesCollection>(
                () => CreatePropertiesCollection());
        }

        protected virtual SwCutListCustomPropertiesCollection CreatePropertiesCollection()
            => new SwCutListCustomPropertiesCollection(m_Doc, Feature.CustomPropertyManager, m_OwnerDoc, m_OwnerConf);

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

        public CutListState_e State 
        {
            get 
            {
                if (Feature.ExcludeFromCutList)
                {
                    return CutListState_e.ExcludeFromBom;
                }
                else 
                {
                    return 0;
                }
            }
        }

        internal void SetOwner(ISwDocument3D doc, ISwConfiguration conf) 
        {
            m_OwnerDoc = doc;
            m_OwnerConf = conf;
        }

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
        private readonly ISwDocument3D m_OwnerDoc;
        private readonly ISwConfiguration m_OwnerConf;

        internal SwCutListCustomPropertiesCollection(ISwDocument doc, CustomPropertyManager prpsMgr,
            ISwDocument3D ownerDoc, ISwConfiguration ownerConf) 
            : base((SwDocument)doc)
        {
            PrpMgr = prpsMgr;

            m_OwnerDoc = ownerDoc;
            m_OwnerConf = ownerConf;
        }

        protected override CustomPropertyManager PrpMgr { get; }

        protected override EventsHandler<PropertyValueChangedDelegate> CreateEventsHandler(SwCustomProperty prp)
            => new CutListCustomPropertyChangeEventsHandler();

        protected override SwCustomProperty CreatePropertyInstance(CustomPropertyManager prpMgr, string name, bool isCreated)
        {
            var prp = new SwCutListCustomProperty(prpMgr, name, m_OwnerDoc, m_OwnerConf, isCreated, m_Doc.App);
            prp.SetEventsHandler(CreateEventsHandler(prp));
            return prp;
        }

        protected override void DeleteProperty(IXProperty prp)
        {
            if (m_OwnerDoc.Configurations.Active.Configuration != m_OwnerConf.Configuration)
            {
                throw new ConfigurationSpecificCutListPropertiesWriteNotSupportedException();
            }

            base.DeleteProperty(prp);
        }
    }

    internal class SwCutListCustomProperty : SwCustomProperty
    {
        private readonly ISwDocument3D m_RefDoc;
        private readonly ISwConfiguration m_RefConf;

        internal SwCutListCustomProperty(CustomPropertyManager prpMgr, string name,
            ISwDocument3D refDoc, ISwConfiguration refConf, bool isCommited, ISwApplication app) 
            : base(prpMgr, name, isCommited, app)
        {
            m_RefDoc = refDoc;
            m_RefConf = refConf;
        }

        protected override void AddProperty(ICustomPropertyManager prpMgr, string name, object value)
        {
            if (m_RefDoc.Configurations.Active.Configuration != m_RefConf.Configuration) 
            {
                throw new ConfigurationSpecificCutListPropertiesWriteNotSupportedException();
            }

            base.AddProperty(prpMgr, name, value);
        }

        protected override void SetProperty(ICustomPropertyManager prpMgr, string name, object value)
        {
            if (m_RefDoc.Configurations.Active.Configuration != m_RefConf.Configuration)
            {
                throw new ConfigurationSpecificCutListPropertiesWriteNotSupportedException();
            }

            base.SetProperty(prpMgr, name, value);
        }
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
