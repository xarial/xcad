//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using Xarial.XCad.Features;

namespace Xarial.XCad.SolidWorks.Features
{
    public class SwSketch2D : SwSketchBase, IXSketch2D
    {
        public SwSketch2D(IModelDoc2 model, IFeature feat, bool created) : base(model, feat, created)
        {
        }

        protected override ISketch CreateSketch()
        {
            //TODO: select the plane or face
            m_ModelDoc.InsertSketch2(true);
            return m_ModelDoc.SketchManager.ActiveSketch;
        }

        protected override void ToggleEditSketch()
        {
            m_ModelDoc.InsertSketch2(true);
        }
    }
}