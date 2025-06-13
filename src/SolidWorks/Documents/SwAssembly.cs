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
using System.Linq;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents.Exceptions;
using Xarial.XCad.SolidWorks.Documents.Services;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Utils.Diagnostics;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwAssembly : ISwDocument3D, IXAssembly
    {
        IAssemblyDoc Assembly { get; }
        new ISwComponent EditingComponent { get; }
        new ISwAssemblyConfigurationCollection Configurations { get; }
    }

    internal class SwAssembly : SwDocument3D, ISwAssembly
    {
        public event ComponentInsertedDelegate ComponentInserted
        {
            add => m_ComponentInsertedEventsHandler.Attach(value);
            remove => m_ComponentInsertedEventsHandler.Detach(value);
        }

        public event ComponentDeletingDelegate ComponentDeleting
        {
            add => m_ComponentDeletingEventsHandler.Attach(value);
            remove => m_ComponentDeletingEventsHandler.Detach(value);
        }

        public event ComponentDeletedDelegate ComponentDeleted
        {
            add => m_ComponentDeletedEventsHandler.Attach(value);
            remove => m_ComponentDeletedEventsHandler.Detach(value);
        }

        public IAssemblyDoc Assembly => Model as IAssemblyDoc;

        private readonly Lazy<SwAssemblyConfigurationCollection> m_LazyConfigurations;
        private readonly SwAssemblyEvaluation m_Evaluation;

        private readonly ComponentInsertedEventsHandler m_ComponentInsertedEventsHandler;
        private readonly ComponentDeletingEventsHandler m_ComponentDeletingEventsHandler;
        private readonly ComponentDeletedEventsHandler m_ComponentDeletedEventsHandler;

        internal SwAssembly(IAssemblyDoc assembly, SwApplication app, IXLogger logger, bool isCreated)
            : base((IModelDoc2)assembly, app, logger, isCreated)
        {
            m_ComponentInsertedEventsHandler = new ComponentInsertedEventsHandler(this, app);
            m_ComponentDeletingEventsHandler = new ComponentDeletingEventsHandler(this, app);
            m_ComponentDeletedEventsHandler = new ComponentDeletedEventsHandler(this, app, logger);

            m_LazyConfigurations = new Lazy<SwAssemblyConfigurationCollection>(() => new SwAssemblyConfigurationCollection(this, app));
            m_Evaluation = new SwAssemblyEvaluation(this);
        }

        internal void BatchSetState(IComponent2[] comps, ComponentState_e state, bool setFixedState)
        {
            swComponentSuppressionState_e suppression;

            if (!state.HasFlag(ComponentState_e.Lightweight))
            {
                suppression = SwComponent.ConvertSuppressionState(state);
            }
            else 
            {
                //NOTE: API limitation - lightweight cannot be set via CompConfigPropertiesX
                suppression = swComponentSuppressionState_e.swComponentFullyResolved;
            }

            var solving = state.HasFlag(ComponentState_e.Flexible) ? swComponentSolvingOption_e.swComponentFlexibleSolving : swComponentSolvingOption_e.swComponentRigidSolving;
            var hidden = state.HasFlag(ComponentState_e.Hidden);
            var exlFromBom = state.HasFlag(ComponentState_e.ExcludedFromBom);
            var envelope = state.HasFlag(ComponentState_e.ExcludedFromBom);

            foreach (var compConfGrp in comps.GroupBy(c => c.ReferencedConfiguration, StringComparer.CurrentCultureIgnoreCase))
            {
                using (var sel = new SelectionGroup(this, true))
                {
                    sel.AddRange(compConfGrp.ToArray());

                    //NOTE: need to specify the reference configuration in CompConfigPropertiesX, otehrwise if empty string is used, configuration is set to default (not the current referenced)
                    var refConfName = compConfGrp.Key;

                    bool compConfigChangeRes;

                    if (OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2019))
                    {
                        compConfigChangeRes = Assembly.CompConfigProperties6((int)suppression, (int)solving, !hidden, true, refConfName, exlFromBom, envelope, (int)swASMSLDPRTCompPref_e.swUseSystemSettings);
                    }
                    else if (OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2017))
                    {
                        compConfigChangeRes = Assembly.CompConfigProperties5((int)suppression, (int)solving, !hidden, true, refConfName, exlFromBom, envelope);
                    }
                    else
                    {
                        throw new NotSupportedException("Batch configuration change is only supported from SOLIDWORKS 2017");
                    }

                    if (!compConfigChangeRes)
                    {
                        throw new Exception("Failed to change configuration component properties");
                    }
                }
            }

            if (state.HasFlag(ComponentState_e.Lightweight)) 
            {
                foreach (var comp in comps) 
                {
                    if (comp.SetSuppression2((int)swComponentSuppressionState_e.swComponentFullyLightweight) != (int)swSuppressionError_e.swSuppressionChangeOk)
                    {
                        throw new Exception($"Failed to set component to lightweight");
                    }
                }
            }

            if (setFixedState)
            {
                //NOTE: selection is cleared after previous portion of code is run, so need to reselect components
                using (var sel = new SelectionGroup(this, true))
                {
                    sel.AddRange(comps);

                    if (state.HasFlag(ComponentState_e.Fixed))
                    {
                        Assembly.FixComponent();
                    }
                    else
                    {
                        Assembly.UnfixComponent();
                    }
                }
            }
        }

        ISwAssemblyConfigurationCollection ISwAssembly.Configurations => m_LazyConfigurations.Value;
        IXAssemblyConfigurationRepository IXAssembly.Configurations => (this as ISwAssembly).Configurations;
        IXComponent IXAssembly.EditingComponent { get => EditingComponent; set => EditingComponent = (ISwComponent)value; }
        IXAssemblyEvaluation IXAssembly.Evaluation => m_Evaluation;

        internal protected override swDocumentTypes_e? DocumentType => swDocumentTypes_e.swDocASSEMBLY;

        protected override bool IsRapidMode => Model.IsOpenedViewOnly(); //TODO: when editing feature of LDR is available make this to be rapid mode

        protected override bool IsLightweightMode => Assembly.GetLightWeightComponentCount() > 0;

        public override IXDocumentEvaluation Evaluation => m_Evaluation;

        public ISwComponent EditingComponent 
        {
            get
            {
                var comp = Assembly.GetEditTargetComponent();

                if (comp != null && !comp.IsRoot())
                {
                    return this.CreateObjectFromDispatch<ISwComponent>(comp);
                }
                else
                {
                    return null;
                }
            }
            set 
            {
                if (value != null)
                {
                    value.Edit();
                }
                else 
                {
                    Model.ClearSelection2(true);
                    Assembly.EditAssembly();
                }
            }
        }

        protected override void CommitCache(IModelDoc2 model, CancellationToken cancellationToken)
        {
            base.CommitCache(model, cancellationToken);

            if (m_LazyConfigurations.IsValueCreated) 
            {
                if (m_LazyConfigurations.Value.ActiveNonCommittedConfigurationLazy.IsValueCreated) 
                {
                    ((SwComponentCollection)((SwAssemblyConfiguration)m_LazyConfigurations.Value.ActiveNonCommittedConfigurationLazy.Value).Components)
                        .CommitCache(cancellationToken);
                }
            }
        }

        protected override SwAnnotationCollection CreateAnnotations()
            => new SwDocument3DAnnotationCollection(this);

        protected override SwConfigurationCollection CreateConfigurations()
            => new SwAssemblyConfigurationCollection(this, OwnerApplication);

        protected override bool IsDocumentTypeCompatible(swDocumentTypes_e docType) => docType == swDocumentTypes_e.swDocASSEMBLY;
    }

    internal class SwAssemblyComponentCollection : SwComponentCollection
    {
        private readonly SwAssembly m_Assm;

        private readonly IConfiguration m_Conf;

        public SwAssemblyComponentCollection(SwAssembly assm, IConfiguration conf) : base(assm)
        {
            m_Assm = assm;
            m_Conf = conf;
        }

        protected bool IsActiveConfiguration => m_Assm.Model.GetActiveConfiguration() == m_Conf;

        protected override bool TryGetByName(string name, out IXComponent ent)
        {
            var comp = RootAssembly.Assembly.GetComponentByName(name);

            if (comp != null)
            {
                if (!IsActiveConfiguration)
                {
                    var rootComp = m_Conf.GetRootComponent3(true);

                    var compId = comp.GetID();

                    comp = null;

                    //finding the correspodning configuration specific component

                    foreach (var corrComp in (rootComp.GetChildren() as object[] ?? new object[0]).Cast<Component2>())
                    {
                        if (corrComp.GetID() == compId) 
                        {
                            comp = corrComp;
                            break;
                        }
                    }
                }
            }

            if (comp != null)
            {
                ent = RootAssembly.CreateObjectFromDispatch<SwComponent>(comp);
                return true;
            }
            else
            {
                ent = null;
                return false;
            }
        }

        protected override IEnumerable<IComponent2> IterateChildren(bool ordered)
        {
            ValidateSpeedPak();

            if (ordered)
            {
                return new OrderedComponentsCollection(
                        () => IterateUnorderedComponents().ToArray(),
                        m_Assm.Model.IFirstFeature(),
                        m_Assm.OwnerApplication.Logger);
            }
            else 
            {
                return IterateUnorderedComponents();
            }
        }

        private IEnumerable<IComponent2> IterateUnorderedComponents()
            => (m_Conf.GetRootComponent3(!IsActiveConfiguration).GetChildren() as object[] ?? new object[0]).Cast<IComponent2>();

        protected override int GetTotalChildrenCount()
        {
            ValidateSpeedPak();
            return m_Assm.Assembly.GetComponentCount(false);
        }
        
        protected override int GetChildrenCount()
        {
            ValidateSpeedPak();
            return m_Assm.Assembly.GetComponentCount(true);
        }

        private void ValidateSpeedPak() 
        {
            if (m_Conf.IsSpeedPak())
            {
                throw new SpeedPakConfigurationComponentsException();
            }
        }
    }
}