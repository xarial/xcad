//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
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
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.SolidWorks.Features
{
    /// <summary>
    /// SOLIDWORKS specific cut-list item
    /// </summary>
    public interface ISwCutListItem : IXCutListItem, ISwFeature, IDisposable
    {
        /// <summary>
        /// Native SOLIDWORKS cut-list item
        /// </summary>
        ICutListItem CutListItem { get; }

        /// <summary>
        /// Pointer to cut-list folder
        /// </summary>
        IBodyFolder CutListBodyFolder { get; }
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwCutListItem : SwFeature, ISwCutListItem
    {
        private readonly Lazy<ISwCustomPropertiesCollection> m_Properties;

        IXPropertyRepository IXCutListItem.Properties => Properties;

        private SwDocument3D m_ParentDoc;
        private ISwConfiguration m_ParentConf;

        private readonly Lazy<ICutListItem> m_CutListItemLazy;

        internal SwCutListItem(IFeature feat, SwDocument3D doc, SwApplication app, bool created) 
            : base(feat, doc, app, created)
        {
            if (feat.GetTypeName2() != "CutListFolder") 
            {
                throw new InvalidCastException("Specified feature is not a cut-list feature");
            }

            m_ParentDoc = doc;
            m_ParentConf = doc.Configurations.Active;

            CutListBodyFolder = (IBodyFolder)feat.GetSpecificFeature2();

            m_Properties = new Lazy<ISwCustomPropertiesCollection>(CreatePropertiesCollection);

            if (OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2024))
            {
                m_CutListItemLazy = new Lazy<ICutListItem>(() =>
                {
                    if (m_ParentConf.IsCommitted)
                    {
                        var cutLists = (object[])m_ParentConf.Configuration.GetCutListItems();
                        if (cutLists != null)
                        {
                            var cutList = (ICutListItem)cutLists.FirstOrDefault(c => string.Equals(((IFeature)c).Name, feat.Name,
                                StringComparison.CurrentCultureIgnoreCase));

                            if (cutList != null)
                            {
                                return cutList;
                            }
                            else
                            {
                                throw new Exception("Failed to find cut list item by name");
                            }
                        }
                    }

                    return null;
                });
            }
        }

        internal SwCutListItem(ICutListItem cutListItem, SwDocument3D doc, SwApplication app, bool created) 
            : this((IFeature)cutListItem, doc, app, created)
        {
            m_CutListItemLazy = new Lazy<ICutListItem>(() => cutListItem);
        }

        private SwCutListCustomPropertiesCollection CreatePropertiesCollection()
            => new SwCutListCustomPropertiesCollection(this, m_ParentDoc, m_ParentConf, OwnerApplication);

        public IBodyFolder CutListBodyFolder { get; }

        public override object Dispatch => CutListBodyFolder;

        public IEnumerable<IXSolidBody> Bodies 
        {
            get
            {
                var comp = Component;

                var bodies = CutListBodyFolder.GetBodies() as object[];

                if (bodies != null)
                {
                    foreach (var body in bodies.Select(OwnerDocument.CreateObjectFromDispatch<ISwSolidBody>)) 
                    {
                        if (comp != null && body.Component == null)
                        {
                            //NOTE: pointer to bodies returned in the assembly context are from part context, need to convert explicitly
                            yield return comp.ConvertObject(body);
                        }
                        else
                        {
                            yield return body;
                        }
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

        public CutListType_e Type 
        {
            get 
            {
                switch ((swCutListType_e)CutListBodyFolder.GetCutListType())
                {
                    case swCutListType_e.swSolidBodyCutList:
                        return CutListType_e.SolidBody;

                    case swCutListType_e.swSheetmetalCutlist:
                        return CutListType_e.SheetMetal;

                    case swCutListType_e.swWeldmentCutlist:
                        return CutListType_e.Weldment;

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public ICutListItem CutListItem 
        {
            get 
            {
                if (OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2024))
                {
                    return m_CutListItemLazy.Value;
                }
                else 
                {
                    throw new NotSupportedException("Native cut-list item is available in SOLIDWORKS 2024 and newer");
                }
            }
        }

        internal void SetParent(SwDocument3D doc, ISwConfiguration conf) 
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

        public void Update()
        {
            if (!CutListBodyFolder.UpdateCutList()) 
            {
                throw new Exception("Failed to update cut-list folder");
            }
        }
    }

    internal class SwCutListCustomPropertiesCollection : SwCustomPropertiesCollection
    {
        private readonly SwDocument3D m_ParentDoc;
        private readonly ISwConfiguration m_ParentConf;

        private readonly SwCutListItem m_CutListItem;

        internal SwCutListCustomPropertiesCollection(SwCutListItem cutListItem,
            SwDocument3D parentDoc, ISwConfiguration parentConf, SwApplication app) 
            : base(app)
        {
            m_CutListItem = cutListItem;

            m_ParentDoc = parentDoc;
            m_ParentConf = parentConf;
        }

        public override IXObject Owner => m_CutListItem;

        protected override CustomPropertyManager PrpMgr
        {
            get
            {
                //NOTE: CutListItem can be null in the lightweight component
                if (m_ParentDoc.OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2024) && m_CutListItem.CutListItem != null)
                {
                    return m_CutListItem.CutListItem.CustomPropertyManager;
                }
                else
                {
                    return m_CutListItem.Feature.CustomPropertyManager;
                }
            }
        }

        protected override EventsHandler<PropertyValueChangedDelegate> CreateEventsHandler(SwCustomProperty prp)
            => new CutListCustomPropertyChangeEventsHandler();

        protected override SwCustomProperty CreatePropertyInstance(string name, bool isCreated)
        {
            var prp = new SwCutListCustomProperty(() => PrpMgr, name, m_ParentDoc, m_ParentConf, isCreated, m_App);
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

        internal SwCutListCustomProperty(Func<CustomPropertyManager> prpMgrFact, string name,
            SwDocument3D refDoc, ISwConfiguration refConf, bool isCommited, SwApplication app) 
            : base(prpMgrFact, name, isCommited, refDoc, app)
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
