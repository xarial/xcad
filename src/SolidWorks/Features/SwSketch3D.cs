//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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

    internal class SwSketch3DEditor : SwSketchEditorBase<SwSketch3D>
    {
        public SwSketch3DEditor(SwSketch3D sketch, ISketch swSketch) : base(sketch, swSketch)
        {
        }

        protected override void StartEdit() => Target.OwnerDocument.Model.SketchManager.Insert3DSketch(true);
        protected override void EndEdit(bool cancel) => Target.OwnerDocument.Model.SketchManager.Insert3DSketch(!cancel);
    }

    internal class SwSketch3D : SwSketchBase, ISwSketch3D
    {
        internal SwSketch3D(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(feat, doc, app, created)
        {
        }

        internal SwSketch3D(ISketch sketch, SwDocument doc, SwApplication app, bool created) : base(sketch, doc, app, created)
        {
        }

        protected internal override IEditor<IXSketchBase> CreateSketchEditor(ISketch sketch) => new SwSketch3DEditor(this, sketch);

        protected override ISketch CreateSketch()
        {
            //TODO: try to use API only selection
            OwnerModelDoc.ClearSelection2(true);
            OwnerModelDoc.Insert3DSketch2(true);
            return OwnerModelDoc.SketchManager.ActiveSketch;
        }
    }
}