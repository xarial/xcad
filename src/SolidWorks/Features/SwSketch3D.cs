//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.Features;

namespace Xarial.XCad.SolidWorks.Features
{
    public class SwSketch3D : SwSketchBase, IXSketch3D
    {
        public SwSketch3D(IModelDoc2 model, IFeature feat, bool created) : base(model, feat, created)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
        }

        protected override ISketch CreateSketch()
        {
            //TODO: try to use API only selection
            m_ModelDoc.ClearSelection2(true);
            m_ModelDoc.Insert3DSketch2(true);
            return m_ModelDoc.SketchManager.ActiveSketch;
        }

        protected override void ToggleEditSketch()
        {
            m_ModelDoc.Insert3DSketch2(true);
        }
    }
}