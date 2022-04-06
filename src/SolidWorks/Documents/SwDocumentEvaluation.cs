using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Geometry;

namespace Xarial.XCad.SolidWorks.Documents
{
    /// <inheritdoc/>
    public interface ISwDocumentEvaluation : IXDocumentEvaluation
    {
    }

    public interface ISwAssemblyEvaluation : ISwDocumentEvaluation, IXAssemblyEvaluation 
    {
    }

    internal abstract class SwDocumentEvaluation : ISwDocumentEvaluation
    {
        private readonly SwDocument3D m_Doc;
        protected readonly IMathUtility m_MathUtils;

        internal SwDocumentEvaluation(SwDocument3D doc) 
        {
            m_Doc = doc;
            m_MathUtils = m_Doc.OwnerApplication.Sw.IGetMathUtility();
        }

        public abstract IXBoundingBox PreCreateBoundingBox();

        public virtual IXMassProperty PreCreateMassProperty()
        {
            if (m_Doc.OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2020))
            {
                return new SwMassProperty(m_Doc, m_MathUtils);
            }
            else
            {
                return new SwLegacyMassProperty(m_Doc, m_MathUtils);
            }
        }
    }

    internal class SwPartEvaluation : SwDocumentEvaluation 
    {
        private readonly SwPart m_Part;

        internal SwPartEvaluation(SwPart part) : base(part) 
        {
            m_Part = part;
        }

        public override IXBoundingBox PreCreateBoundingBox()
            => new SwPartBoundingBox(m_Part, m_Part.OwnerApplication);
    }

    internal class SwAssemblyEvaluation : SwDocumentEvaluation, ISwAssemblyEvaluation
    {
        private readonly SwAssembly m_Assm;

        internal SwAssemblyEvaluation(SwAssembly assm) : base(assm) 
        {
            m_Assm = assm;
        }

        public override IXBoundingBox PreCreateBoundingBox()
            => (this as IXAssemblyEvaluation).PreCreateBoundingBox();

        IXAssemblyBoundingBox IXAssemblyEvaluation.PreCreateBoundingBox()
            => new SwAssemblyBoundingBox(m_Assm, m_Assm.OwnerApplication);

        public override IXMassProperty PreCreateMassProperty()
            => (this as IXAssemblyEvaluation).PreCreateMassProperty();

        IXAssemblyMassProperty IXAssemblyEvaluation.PreCreateMassProperty()
        {
            if (m_Assm.OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2020))
            {
                return new SwAssemblyMassProperty(m_Assm, m_MathUtils);
            }
            else
            {
                return new SwAssemblyLegacyMassProperty(m_Assm, m_MathUtils);
            }
        }
    }
}
