using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Annotations;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Annotations
{
    public interface ISwAnnotation : IXAnnotation, ISwSelObject
    {
        IAnnotation Annotation { get; }
    }

    internal class SwAnnotation : SwSelObject, ISwAnnotation
    {
        public IAnnotation Annotation { get; }

        internal SwAnnotation(IAnnotation ann, ISwDocument doc, ISwApplication app) : base(ann, doc, app) 
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
    }
}
