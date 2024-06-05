//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Evaluation;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Geometry.Evaluation;

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
        protected readonly Lazy<IMathUtility> m_MathUtilsLazy;

        internal SwDocumentEvaluation(SwDocument3D doc) 
        {
            m_Doc = doc;
            m_MathUtilsLazy = new Lazy<IMathUtility>(m_Doc.OwnerApplication.Sw.IGetMathUtility);
        }

        public abstract IXBoundingBox PreCreateBoundingBox();

        public virtual IXMassProperty PreCreateMassProperty()
        {
            if (m_Doc.OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2020))
            {
                return new SwMassProperty(m_Doc, m_MathUtilsLazy.Value);
            }
            else
            {
                return new SwLegacyMassProperty(m_Doc, m_MathUtilsLazy.Value);
            }
        }

        public abstract IXRayIntersection PreCreateRayIntersection();

        public abstract IXTessellation PreCreateTessellation();

        public virtual IXCollisionDetection PreCreateCollisionDetection() => new SwCollisionDetection(m_Doc, m_Doc.OwnerApplication);
        
        public IXMeasure PreCreateMeasure() => throw new NotImplementedException();

        public IXFaceTesselation PreCreateFaceTessellation() => new SwFaceTesselation(m_Doc);
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

        public override IXRayIntersection PreCreateRayIntersection()
        {
            if (m_Part.OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2018, 2))
            {
                return new SwPartRayIntersection(m_Part);
            }
            else
            {
                throw new NotSupportedException("Supported from SOLIDWORKS 2018 SP2");
            }
        }

        public override IXTessellation PreCreateTessellation() => new SwPartTesselation(m_Part);
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
                return new SwAssemblyMassProperty(m_Assm, m_MathUtilsLazy.Value);
            }
            else
            {
                return new SwAssemblyLegacyMassProperty(m_Assm, m_MathUtilsLazy.Value);
            }
        }

        IXAssemblyRayIntersection IXAssemblyEvaluation.PreCreateRayIntersection()
        {
            if (m_Assm.OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2018, 2))
            {
                return new SwAssemblyRayIntersection(m_Assm);
            }
            else 
            {
                throw new NotSupportedException("Supported from SOLIDWORKS 2018 SP2");
            }
        }

        public override IXRayIntersection PreCreateRayIntersection()
            => (this as IXAssemblyEvaluation).PreCreateRayIntersection();

        public override IXTessellation PreCreateTessellation()
            => (this as IXAssemblyEvaluation).PreCreateTessellation();

        IXAssemblyTessellation IXAssemblyEvaluation.PreCreateTessellation()
            => new SwAssemblyTesselation(m_Assm);

        IXAssemblyCollisionDetection IXAssemblyEvaluation.PreCreateCollisionDetection() 
            => new SwAssemblyCollisionDetection(m_Assm, m_Assm.OwnerApplication);
    }
}
