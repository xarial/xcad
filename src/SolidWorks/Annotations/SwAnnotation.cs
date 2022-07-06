using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Annotations;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Annotations
{
    public interface ISwAnnotation : IXAnnotation, ISwSelObject
    {
        IAnnotation Annotation { get; }
    }

    internal class SwAnnotation : SwSelObject, ISwAnnotation
    {
        public IAnnotation Annotation { get; }

        internal SwAnnotation(IAnnotation ann, SwDocument doc, SwApplication app) : base(ann, doc, app) 
        {
            Annotation = ann;
        }

        public Point Position
        {
            get => new Point((double[])Annotation.GetPosition());
            set
            {
                if (OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2014, 3))
                {
                    if (!Annotation.SetPosition2(value.X, value.Y, value.Z))
                    {
                        throw new Exception("Failed to set the position of the dimension");
                    }
                }
                else
                {
                    if (!Annotation.SetPosition(value.X, value.Y, value.Z))
                    {
                        throw new Exception("Failed to set the position of the dimension");
                    }
                }
            }
        }

        public System.Drawing.Color? Color 
        {
            get
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
            set
            {
                if (value.HasValue)
                {
                    Annotation.Color = ColorUtils.ToColorRef(value.Value);
                }
                else 
                {
                    var layerOverride = (swLayerOverride_e)Annotation.LayerOverride;

                    if (layerOverride.HasFlag(swLayerOverride_e.swLayerOverrideColor)) 
                    {
                        layerOverride -= swLayerOverride_e.swLayerOverrideColor;
                    }

                    Annotation.LayerOverride = (int)layerOverride;
                }
            }
        }
    }
}
