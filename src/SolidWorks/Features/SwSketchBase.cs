//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Xarial.XCad.Features;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Sketch;

namespace Xarial.XCad.SolidWorks.Features
{
    public interface ISwSketchBase : IXSketchBase, ISwFeature
    {
        ISketch Sketch { get; }
    }

    internal abstract class SwSketchEditorBase<TSketch> : IEditor<TSketch>
        where TSketch : SwSketchBase
    {
        public TSketch Target { get; }

        public bool Cancel
        {
            get => false;
            set => throw new NotSupportedException("This operation cannot be cancelled");
        }

        protected abstract void StartEdit();
        protected abstract void EndEdit(bool cancel);

        private readonly bool? m_AddToDbOrig;

        private readonly ISketchManager m_SketchMgr;

        private readonly ISketch m_Sketch;

        protected SwSketchEditorBase(TSketch sketch, ISketch swSketch) 
        {
            if (sketch == null)
            {
                throw new ArgumentNullException(nameof(sketch));
            }

            if (swSketch == null)
            {
                throw new ArgumentNullException(nameof(swSketch));
            }

            Target = sketch;
            m_Sketch = swSketch;

            m_SketchMgr = Target.OwnerDocument.Model.SketchManager;

            if (!Target.IsEditing)
            {
                if (((IFeature)m_Sketch).Select2(false, 0))
                {
                    m_AddToDbOrig = m_SketchMgr.AddToDB;
                    StartEdit();
                }
                else 
                {
                    throw new Exception("Failed to select sketch for editing");
                }
            }
        }

        public void Dispose()
        {
            if (Target.IsEditing)
            {
                if (m_AddToDbOrig.HasValue)
                {
                    m_SketchMgr.AddToDB = m_AddToDbOrig.Value;
                }

                m_SketchMgr.Document.ClearSelection2(true);
                
                EndEdit(Cancel);
            }
        }
    }

    internal abstract class SwSketchBase : SwFeature, ISwSketchBase
    {
        private readonly SwSketchEntityCollection m_SwEntsColl;

        public ISketch Sketch { get; private set; }

        public override object Dispatch => Sketch;

        internal SwSketchBase(IFeature feat, ISwDocument doc, ISwApplication app, bool created) 
            : this(feat, (ISketch)feat?.GetSpecificFeature2(), doc, app, created)
        {
        }

        internal SwSketchBase(ISketch sketch, ISwDocument doc, ISwApplication app, bool created) : this((IFeature)sketch, sketch, doc, app, created)
        {
        }

        private SwSketchBase(IFeature feat, ISketch sketch, ISwDocument doc, ISwApplication app, bool created) : base(feat, doc, app, created)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            m_SwEntsColl = new SwSketchEntityCollection(this, doc, app);
            Sketch = sketch;
        }

        public IXSketchEntityRepository Entities => m_SwEntsColl;

        protected override IFeature CreateFeature(CancellationToken cancellationToken)
        {
            var sketch = CreateSketch();

            Sketch = sketch;

            return (IFeature)sketch;
        }

        protected override void CommitCache(IFeature feat, CancellationToken cancellationToken)
        {
            m_SwEntsColl.CommitCache(cancellationToken);
        }

        protected abstract ISketch CreateSketch();

        public override IEditor<IXFeature> Edit() => CreateSketchEditor(Sketch);

        protected internal bool IsEditing => OwnerDocument.Model.SketchManager.ActiveSketch == Sketch;

        protected internal abstract IEditor<IXSketchBase> CreateSketchEditor(ISketch sketch);
    }
}