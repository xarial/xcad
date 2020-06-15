//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.Services;
using Xarial.XCad.Sketch;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public abstract class SwSketchEntity : SwSelObject, IXSketchEntity
    {
        internal abstract void Create();

        internal SwSketchEntity(IModelDoc2 model, object ent) : base(model, ent)
        {
        }
    }

    public abstract class SwSketchEntity<TEnt> : SwSketchEntity
    {
        protected readonly ElementCreator<TEnt> m_Creator;

        protected readonly ISketchManager m_SketchMgr;

        public TEnt Element
        {
            get
            {
                return m_Creator.Element;
            }
        }

        internal SwSketchEntity(IModelDoc2 model, TEnt ent, bool created) : base(model, ent)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            m_SketchMgr = model.SketchManager;
            m_Creator = new ElementCreator<TEnt>(CreateSketchEntity, ent, created);
        }

        internal override void Create()
        {
            m_Creator.Create();
        }

        protected abstract TEnt CreateSketchEntity();
    }
}