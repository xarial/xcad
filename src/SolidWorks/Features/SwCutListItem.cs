//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwCutListItem : SwFeature, ISwCutListItem
    {
        private readonly Lazy<ISwCustomPropertiesCollection> m_Properties;

        IXPropertyRepository IPropertiesOwner.Properties => Properties;

        private ISwDocument3D m_ParentDoc;
        private ISwConfiguration m_ParentConf;
        
        internal SwCutListItem(IFeature feat, SwDocument3D doc, SwApplication app, bool created) : base(feat, doc, app, created)
        {
            if (feat.GetTypeName2() != "CutListFolder") 
            {
                throw new InvalidCastException("Specified feature is not a cut-list feature");
            }

            m_ParentDoc = doc;
            m_ParentConf = doc.Configurations.Active;

            CutListBodyFolder = (IBodyFolder)feat.GetSpecificFeature2();

            m_Properties = new Lazy<ISwCustomPropertiesCollection>(
                () => CreatePropertiesCollection());
        }

        protected virtual SwCutListCustomPropertiesCollection CreatePropertiesCollection()
            => new SwCutListCustomPropertiesCollection(Feature.CustomPropertyManager, m_ParentDoc, m_ParentConf, OwnerApplication);

        public IBodyFolder CutListBodyFolder { get; }

        public override object Dispatch => CutListBodyFolder;

        public IEnumerable<IXSolidBody> Bodies 
        {
            get
            {
                var bodies = CutListBodyFolder.GetBodies() as object[];

                if (bodies != null)
                {
                    foreach (var body in bodies.Select(b => OwnerDocument.CreateObjectFromDispatch<ISwSolidBody>(b))) 
                    {
                        yield return body;
                    }
                }
            }
        }

        public ISwCustomPropertiesCollection Properties => m_Properties.Value;

        public CutListStatus_e Status 
        {
            get 
            {
                if (Feature.ExcludeFromCutList)
                {
                    return CutListStatus_e.ExcludeFromBom;
                }
                else 
                {
                    return 0;
                }
            }
        }

        internal void SetParent(ISwDocument3D doc, ISwConfiguration conf) 
        {
            m_ParentDoc = doc;
            m_ParentConf = conf;
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
        private readonly ISwDocument3D m_ParentDoc;
        private readonly ISwConfiguration m_ParentConf;

        internal SwCutListCustomPropertiesCollection(CustomPropertyManager prpsMgr,
            ISwDocument3D parentDoc, ISwConfiguration parentConf, ISwApplication app) 
            : base((SwDocument)parentDoc, app)
        {
            PrpMgr = prpsMgr;

            m_ParentDoc = parentDoc;
            m_ParentConf = parentConf;
        }

        protected override CustomPropertyManager PrpMgr { get; }

        protected override EventsHandler<PropertyValueChangedDelegate> CreateEventsHandler(SwCustomProperty prp)
            => new CutListCustomPropertyChangeEventsHandler();

        protected override SwCustomProperty CreatePropertyInstance(CustomPropertyManager prpMgr, string name, bool isCreated)
        {
            var prp = new SwCutListCustomProperty(prpMgr, name, m_ParentDoc, m_ParentConf, isCreated, m_App);
            InitProperty(prp);
            return prp;
        }

        protected override void DeleteProperty(IXProperty prp)
        {
            if (m_ParentDoc.Configurations.Active.Configuration != m_ParentConf.Configuration)
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
