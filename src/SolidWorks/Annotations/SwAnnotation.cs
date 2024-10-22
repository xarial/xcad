//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Forms;
using Xarial.XCad.Annotations;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Annotations
{
    public interface ISwAnnotation : IXAnnotation, ISwSelObject
    {
        IAnnotation Annotation { get; }
    }

    internal class SwAnnotation : SwSelObject, ISwAnnotation
    {
        internal static SwAnnotation New(IAnnotation ann, SwDocument doc, SwApplication app)
        {
            if (doc is IXDrawing)
            {
                return SwDrawingAnnotation.New(ann, (SwDrawing)doc, app);
            }
            else
            {
                return new SwAnnotation(ann, doc, app);
            }
        }

        public virtual IAnnotation Annotation => m_Creator.Element;

        public override object Dispatch => Annotation;

        public override bool IsCommitted => m_Creator.IsCreated;

        protected readonly ElementCreator<IAnnotation> m_Creator;

        protected SwAnnotation(IAnnotation ann, SwDocument doc, SwApplication app) : base(ann, doc, app) 
        {
            m_Creator = new ElementCreator<IAnnotation>(CreateAnnotation, ann, ann != null);
        }

        protected SwAnnotation(Lazy<IAnnotation> lazyAnn, SwDocument doc, SwApplication app) : base(null, doc, app)
        {
            m_Creator = new ElementCreator<IAnnotation>(lazyAnn, CreateAnnotation);
        }

        public Point Position
        {
            get
            {
                if (IsCommitted)
                {
                    return new Point((double[])Annotation.GetPosition());
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<Point>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    SetPosition(Annotation, value);
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public virtual IXLayer Layer
        {
            get => SwLayerHelper.GetLayer(this, x => x.Annotation.Layer);
            set => SwLayerHelper.SetLayer(this, value, (x, y) => x.Annotation.Layer = y);
        }

        public System.Drawing.Color? Color 
        {
            get
            {
                if (IsCommitted)
                {
                    var layerOverride = (swLayerOverride_e)Annotation.LayerOverride;

                    if (layerOverride.HasFlag(swLayerOverride_e.swLayerOverrideColor))
                    {
                        return ColorUtils.FromColorRef(Annotation.Color);
                    }
                    else
                    {
                        return null;
                    }
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<System.Drawing.Color?>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    SetColor(Annotation, value);
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public IFont Font
        {
            get
            {
                if (IsCommitted)
                {
                    return new SwTextFormat((ITextFormat)Annotation.GetTextFormat(0));
                }
                else
                {
                    return m_Creator.CachedProperties.Get<IFont>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    ITextFormat textFormat;

                    if (value is SwTextFormat)
                    {
                        textFormat = ((SwTextFormat)value).TextFormat;
                    }
                    else 
                    {
                        textFormat = (ITextFormat)Annotation.GetTextFormat(0);

                        if (value != null)
                        {
                            textFormat = SwTextFormat.Load(textFormat, value).TextFormat;
                        }
                    }
                    
                    Annotation.SetTextFormat(0, value == null, textFormat);
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public override void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);

        protected virtual IAnnotation CreateAnnotation(CancellationToken arg)
            => throw new NotSupportedException("Creating of this annotation is not supported");

        internal override void Select(bool append, ISelectData selData)
        {
            if (!Annotation.Select3(append, (SelectData)selData)) 
            {
                throw new Exception("Failed to select annotation");
            }
        }

        protected void SetPosition(IAnnotation ann, Point value)
        {
            if (OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2014, 3))
            {
                if (!ann.SetPosition2(value.X, value.Y, value.Z))
                {
                    throw new Exception("Failed to set the position of the dimension");
                }
            }
            else
            {
                if (!ann.SetPosition(value.X, value.Y, value.Z))
                {
                    throw new Exception("Failed to set the position of the dimension");
                }
            }
        }

        protected void SetColor(IAnnotation ann, System.Drawing.Color? value)
        {
            if (value.HasValue)
            {
                ann.Color = ColorUtils.ToColorRef(value.Value);
            }
            else
            {
                var layerOverride = (swLayerOverride_e)ann.LayerOverride;

                if (layerOverride.HasFlag(swLayerOverride_e.swLayerOverrideColor))
                {
                    layerOverride -= swLayerOverride_e.swLayerOverrideColor;
                }

                ann.LayerOverride = (int)layerOverride;
            }
        }

        protected void Refresh(IAnnotation ann) 
        {
            var origVisible = ann.Visible;

            if (origVisible != (int)swAnnotationVisibilityState_e.swAnnotationHidden)
            {
                ann.Visible = (int)swAnnotationVisibilityState_e.swAnnotationHidden;

                ann.Visible = origVisible;
            }
        }
    }

    internal class SwDrawingAnnotation : SwAnnotation, IXDrawingAnnotation
    {
        internal static SwDrawingAnnotation New(IAnnotation ann, SwDrawing drw, SwApplication app)
            => new SwDrawingAnnotation(ann, drw, app);

        public IXObject Owner 
        {
            get => m_DrwAnnWrapper.Owner; 
            set => m_DrwAnnWrapper.Owner = value; 
        }

        private readonly SwDrawingAnnotationWrapper m_DrwAnnWrapper;

        protected SwDrawingAnnotation(IAnnotation ann, SwDrawing drw, SwApplication app) : base(ann, drw, app)
        {
            m_DrwAnnWrapper = new SwDrawingAnnotationWrapper(this);
        }
    }

    internal class SwDrawingAnnotationWrapper
    {
        private readonly SwAnnotation m_Ann;

        public IXObject Owner 
        {
            get 
            {
                switch ((swAnnotationOwner_e)m_Ann.Annotation.OwnerType) 
                {
                    case swAnnotationOwner_e.swAnnotationOwner_Part:
                    case swAnnotationOwner_e.swAnnotationOwner_Assembly:
                        return m_Ann.OwnerDocument;

                    case swAnnotationOwner_e.swAnnotationOwner_DrawingSheet:
                        return m_Ann.OwnerDocument.CreateObjectFromDispatch<ISwSheet>(m_Ann.Annotation.Owner);

                    case swAnnotationOwner_e.swAnnotationOwner_DrawingView:
                        return m_Ann.OwnerDocument.CreateObjectFromDispatch<ISwDrawingView>(m_Ann.Annotation.Owner);

                    case swAnnotationOwner_e.swAnnotationOwner_DrawingTemplate:
                        var sheet = m_Ann.OwnerDocument.CreateObjectFromDispatch<ISwSheet>(m_Ann.Annotation.Owner);
                        return sheet.Format;

                    default:
                        throw new NotSupportedException();
                }
            }
            set 
            {
                switch (value) 
                {
                    case IXSheet sheet:
                        if (((ISwDrawing)m_Ann.OwnerDocument).Sheets.Active.Equals(sheet))
                        {
                            using (var selGrp = new SelectionGroup(m_Ann.OwnerDocument, true))
                            {
                                selGrp.Add(m_Ann.Dispatch);

                                if (!((ISwDrawing)m_Ann.OwnerDocument).Drawing.AttachAnnotation(
                                    (int)swAttachAnnotationOption_e.swAttachAnnotationOption_Sheet))
                                {
                                    throw new Exception("Failed to attach annotation to sheet");
                                }
                            }
                        }
                        else 
                        {
                            throw new Exception("Annootation can be attached to active sheet only");
                        }
                        break;

                    case IXDrawingView view:
                        
                        using (var selGrp = new SelectionGroup(m_Ann.OwnerDocument, true)) 
                        {
                            selGrp.Add(m_Ann.Dispatch);
                            selGrp.Add(((ISwDrawingView)view).Dispatch);
                            
                            if (!((ISwDrawing)m_Ann.OwnerDocument).Drawing.AttachAnnotation(
                                (int)swAttachAnnotationOption_e.swAttachAnnotationOption_View))
                            {
                                throw new Exception("Failed to attach annotation to view");
                            }
                        }
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        internal SwDrawingAnnotationWrapper(SwAnnotation ann)
        {
            m_Ann = ann;
        }
    }
}
