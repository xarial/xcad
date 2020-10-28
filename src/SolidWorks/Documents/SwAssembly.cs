//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Utils.Diagnostics;

namespace Xarial.XCad.SolidWorks.Documents
{
    public class SwAssembly : SwDocument3D, IXAssembly
    {
        public IAssemblyDoc Assembly => Model as IAssemblyDoc;

        public IXComponentRepository Components { get; }

        protected override swUserPreferenceStringValue_e DefaultTemplate => swUserPreferenceStringValue_e.swDefaultTemplateAssembly;

        internal SwAssembly(IAssemblyDoc assembly, SwApplication app, IXLogger logger, bool isCreated)
            : base((IModelDoc2)assembly, app, logger, isCreated)
        {
            Components = new SwAssemblyComponentCollection(this);
        }

        public override Box3D CalculateBoundingBox()
        {
            const int NO_REF_GEOM = 0;

            var box = Assembly.GetBox(NO_REF_GEOM) as double[];

            return new Box3D(box[0], box[1], box[2], box[3], box[4], box[5]);
        }
    }

    internal class SwAssemblyComponentCollection : SwComponentCollection
    {
        private readonly SwAssembly m_Assm;

        public SwAssemblyComponentCollection(SwAssembly assm) : base(assm)
        {
            m_Assm = assm;
        }

        protected override IComponent2 GetRootComponent()
            => (m_Assm.Assembly as IModelDoc2)
                .ConfigurationManager.ActiveConfiguration.GetRootComponent3(true);
    }
}