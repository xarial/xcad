//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xarial.XCad.Annotations;
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
        public IAnnotation Annotation => m_Creator.Element;

        public override bool IsCommitted => m_Creator.IsCreated;

        protected readonly ElementCreator<IAnnotation> m_Creator;

        internal SwAnnotation(IAnnotation ann, SwDocument doc, SwApplication app) : base(ann, doc, app) 
        {
            m_Creator = new ElementCreator<IAnnotation>(CreateAnnotation, ann, ann != null);
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
                    return SwFontHelper.FromTextFormat((ITextFormat)Annotation.GetTextFormat(0));
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
                    var textFormat = (ITextFormat)Annotation.GetTextFormat(0);
                    
                    if (value != null)
                    {
                        SwFontHelper.FillTextFormat(value, textFormat);
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
    }
}
