//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.Utils.Diagnostics;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwAssembly : ISwDocument3D, IXAssembly
    {
        IAssemblyDoc Assembly { get; }
        new ISwAssemblyConfigurationCollection Configurations { get; }
    }

    internal class SwAssembly : SwDocument3D, ISwAssembly
    {
        public IAssemblyDoc Assembly => Model as IAssemblyDoc;

        private readonly Lazy<SwAssemblyConfigurationCollection> m_LazyConfigurations;

        internal SwAssembly(IAssemblyDoc assembly, ISwApplication app, IXLogger logger, bool isCreated)
            : base((IModelDoc2)assembly, app, logger, isCreated)
        {
            m_LazyConfigurations = new Lazy<SwAssemblyConfigurationCollection>(() => new SwAssemblyConfigurationCollection(this, app));
        }

        internal protected override swDocumentTypes_e? DocumentType => swDocumentTypes_e.swDocASSEMBLY;

        protected override bool IsRapidMode => Model.IsOpenedViewOnly(); //TODO: when editing feature of LDR is available make this to be rapid mode

        protected override bool IsLightweightMode => Assembly.GetLightWeightComponentCount() > 0;

        ISwAssemblyConfigurationCollection ISwAssembly.Configurations => m_LazyConfigurations.Value;
        IXAssemblyConfigurationRepository IXAssembly.Configurations => (this as ISwAssembly).Configurations;

        public override IXBoundingBox PreCreateBoundingBox()
            => (this as IXAssembly).PreCreateBoundingBox();

        IXAssemblyBoundingBox IXAssembly.PreCreateBoundingBox()
            => new SwAssemblyBoundingBox(this, OwnerApplication);

        public override IXMassProperty PreCreateMassProperty()
            => (this as IXAssembly).PreCreateMassProperty();

        IXAssemblyMassProperty IXAssembly.PreCreateMassProperty()
        {
            if (OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2020))
            {
                return new SwAssemblyMassProperty(this, m_MathUtils);
            }
            else
            {
                return new SwAssemblyLegacyMassProperty(this, m_MathUtils);
            }
        }
    }

    internal class SwAssemblyComponentCollection : SwComponentCollection
    {
        private readonly ISwAssembly m_Assm;

        private readonly IConfiguration m_Conf;

        public SwAssemblyComponentCollection(ISwAssembly assm, IConfiguration conf) : base(assm)
        {
            m_Assm = assm;
            m_Conf = conf;
        }

        protected override int GetTotalChildrenCount()
            => m_Assm.Assembly.GetComponentCount(false);
        
        protected override IEnumerable<IComponent2> GetChildren()
            => (m_Conf.GetRootComponent3(m_Assm.Model.GetActiveConfiguration() != m_Conf).GetChildren() as object[])?.Cast<IComponent2>();

        protected override int GetChildrenCount() 
            => m_Assm.Assembly.GetComponentCount(true);
    }
}