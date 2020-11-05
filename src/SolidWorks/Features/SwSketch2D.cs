//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.Features;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Features
{
    public interface ISwSketch2D : ISwSketchBase, IXSketch2D
    {
    }

    internal class SwSketch2D : SwSketchBase, ISwSketch2D
    {
        internal SwSketch2D(ISwDocument doc, IFeature feat, bool created) : base(doc, feat, created)
        {
            if (doc == null) 
            {
                throw new ArgumentNullException(nameof(doc));
            }
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