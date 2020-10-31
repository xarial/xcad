//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Drawing;
using Xarial.XCad.Services;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public abstract class SwSketchEntity : SwSelObject, IXSketchEntity
    {
        public abstract bool IsCommitted { get; }
        public abstract Color? Color { get; set; }

        protected readonly SwDocument m_Doc;
        
        internal SwSketchEntity(SwDocument doc, object ent) : base(doc.Model, ent)
        {
            m_Doc = doc;
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

        public override bool IsCommitted => m_Creator.IsCreated;

        internal SwSketchEntity(SwDocument doc, TEnt ent, bool created) : base(doc, ent)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            m_SketchMgr = doc.Model.SketchManager;
            m_Creator = new ElementCreator<TEnt>(CreateEntity, ent, created);
        }

        public override void Commit() => m_Creator.Create();

        private Color? m_CachedColor;

        public override Color? Color 
        {
            get 
            {
                if (IsCommitted)
                {
                    return GetColor();
                }
                else
                {
                    return m_CachedColor;
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    SetColor(Element, value);
                }
                else 
                {
                    m_CachedColor = value;
                }
            }
        }

        private void SetColor(TEnt ent, Color? color) 
        {
            int colorRef = 0;

            if (color.HasValue)
            {
                colorRef = ColorUtils.ToColorRef(color.Value);
            }

            if (ent is ISketchSegment)
            {
                (ent as ISketchSegment).Color = colorRef;
            }
            else if (ent is ISketchPoint)
            {
                (ent as ISketchPoint).Color = colorRef;
            }
            else 
            {
                throw new NotSupportedException("This sketch entity is not supported");
            }
        }

        private Color? GetColor()
        {
            int colorRef = -1;

            if (Element is ISketchSegment)
            {
                colorRef = (Element as ISketchSegment).Color;
            }
            else if (Element is ISketchPoint)
            {
                colorRef = (Element as ISketchPoint).Color;
            }
            else
            {
                throw new NotSupportedException("This sketch entity is not supported");
            }

            return ColorUtils.FromColorRef(colorRef);
        }

        private TEnt CreateEntity() 
        {
            var ent = CreateSketchEntity();

            SetColor(ent, m_CachedColor);

            return ent;
        }

        protected abstract TEnt CreateSketchEntity();
    }
}