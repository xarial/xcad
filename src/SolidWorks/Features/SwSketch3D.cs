//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.Features;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Features
{
    public interface ISwSketch3D : ISwSketchBase, IXSketch3D
    {
    }

    internal class SwSketch3D : SwSketchBase, ISwSketch3D
    {
        internal SwSketch3D(IFeature feat, ISwDocument doc, ISwApplication app, bool created) : base(feat, doc, app, created)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }
        }

        protected override ISketch CreateSketch()
        {
            //TODO: try to use API only selection
            OwnerModelDoc.ClearSelection2(true);
            OwnerModelDoc.Insert3DSketch2(true);
            return OwnerModelDoc.SketchManager.ActiveSketch;
        }

        protected override void ToggleEditSketch()
            => OwnerModelDoc.Insert3DSketch2(true);
    }
}