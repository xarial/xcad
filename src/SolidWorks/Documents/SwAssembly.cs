//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents.Services;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Geometry;
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
        public IAssemblyDoc Assembly => Model as IAssemblyDoc;

        private readonly Lazy<SwAssemblyConfigurationCollection> m_LazyConfigurations;
        private readonly SwAssemblyEvaluation m_Evaluation;

        internal SwAssembly(IAssemblyDoc assembly, SwApplication app, IXLogger logger, bool isCreated)
            : base((IModelDoc2)assembly, app, logger, isCreated)
        {
            m_LazyConfigurations = new Lazy<SwAssemblyConfigurationCollection>(() => new SwAssemblyConfigurationCollection(this, app));
            m_Evaluation = new SwAssemblyEvaluation(this);
        }

        ISwAssemblyConfigurationCollection ISwAssembly.Configurations => m_LazyConfigurations.Value;
        IXAssemblyConfigurationRepository IXAssembly.Configurations => (this as ISwAssembly).Configurations;
        IXComponent IXAssembly.EditingComponent => EditingComponent;
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
        }

        protected override SwAnnotationCollection CreateAnnotations()
            => new SwDocument3DAnnotationCollection(this);

        protected override SwConfigurationCollection CreateConfigurations()
            => new SwAssemblyConfigurationCollection(this, OwnerApplication);

        protected override bool IsDocumentTypeCompatible(swDocumentTypes_e docType) => docType == swDocumentTypes_e.swDocASSEMBLY;
    }

    internal class SwAssemblyComponentCollection : SwComponentCollection
    {
        private readonly ISwAssembly m_Assm;

        private readonly IConfiguration m_Conf;

        public SwAssemblyComponentCollection(SwAssembly assm, IConfiguration conf) : base(assm)
        {
            m_Assm = assm;
            m_Conf = conf;
        }

        protected override IEnumerable<IComponent2> IterateChildren()
        {
            var isActiveConf = m_Assm.Model.GetActiveConfiguration() == m_Conf;

            var compsMapLazy = new Lazy<Dictionary<int, IComponent2>>(() => 
            {
                var rootComp = m_Conf.GetRootComponent3(!isActiveConf);

                //NOTE: GetChildren returns components in the random order, however this seems to be the only way to access the pointer
                //of the components of the inactive configuraiton of the assembly. Collecting of the pointer to later map to the ordered components

                return (rootComp.GetChildren() as object[] ?? new object[0]).Cast<IComponent2>().ToDictionary(c => c.GetID());
            });

            foreach (var feat in base.IterateFeatureComponents(m_Assm.Model.IFirstFeature())) 
            {
                var comp = (IComponent2)feat.GetSpecificFeature2();

                if (!isActiveConf)
                {
                    //components are iterated in the active configuration of the model for the inactive configuration retrieve the corresponding component by id
                    comp = compsMapLazy.Value[comp.GetID()];
                }

                yield return comp;
            }
        }

        protected override int GetTotalChildrenCount() => m_Assm.Assembly.GetComponentCount(false);
        protected override int GetChildrenCount() => m_Assm.Assembly.GetComponentCount(true);
    }
}