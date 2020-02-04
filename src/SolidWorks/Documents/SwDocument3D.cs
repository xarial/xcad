//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Utils.Diagnostics;

namespace Xarial.XCad.SolidWorks.Documents
{
    public abstract class SwDocument3D : SwDocument, IXDocument3D
    {
        private readonly IMathUtility m_MathUtils;

        public IXView ActiveView => new SwModelView(Model, Model.IActiveView, m_MathUtils);

        internal SwDocument3D(IModelDoc2 model, ISldWorks app, ILogger logger) : base(model, app, logger)
        {
            m_MathUtils = app.IGetMathUtility();
        }

        public abstract Box3D CalculateBoundingBox();
    }
}