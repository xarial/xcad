//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.Features;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Sketch;

namespace Xarial.XCad.SolidWorks.Features
{
    public abstract class SwSketchBase : SwFeature, IXSketchBase
    {
        private readonly SwSketchEntityCollection m_SwEntsColl;

        public ISketch Sketch => Feature?.GetSpecificFeature2() as ISketch;

        internal SwSketchBase(IModelDoc2 model, IFeature feat, bool created) : base(model, feat, created)
        {
            m_SwEntsColl = new SwSketchEntityCollection(model, this, model.SketchManager);
        }

        public IXSketchEntityRepository Entities => m_SwEntsColl;

        public bool IsEditing
        {
            get
            {
                if (IsCreated)
                {
                    return GetEditMode(Sketch);
                }
                else
                {
                    throw new Exception("This option is only valid for the committed sketch");
                }
            }
            set
            {
                if (IsCreated)
                {
                    SetEditMode(Sketch, value);
                }
                else
                {
                    throw new Exception("This option is only valid for the committed sketch");
                }
            }
        }

        internal bool GetEditMode(ISketch sketch)
        {
            return m_ModelDoc.SketchManager.ActiveSketch == sketch;
        }

        internal void SetEditMode(ISketch sketch, bool isEditing)
        {
            if (isEditing)
            {
                if (!GetEditMode(sketch))
                {
                    //TODO: use API only selection
                    (sketch as IFeature).Select2(false, 0);
                    ToggleEditSketch();
                }
            }
            else
            {
                if (GetEditMode(sketch))
                {
                    ToggleEditSketch();
                }
            }
        }

        protected abstract void ToggleEditSketch();

        protected override IFeature CreateFeature()
        {
            var sketch = CreateSketch();

            m_SwEntsColl.CommitCache(sketch);

            return (IFeature)sketch;
        }

        protected abstract ISketch CreateSketch();
    }
}