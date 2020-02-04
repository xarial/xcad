//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Geometry;

namespace Xarial.XCad.SolidWorks
{
    public class SwObject : IXObject
    {
        public static SwObject FromDispatch(object disp, IModelDoc2 model = null)
        {
            switch (disp)
            {
                //TODO: make this automatic
                case IEdge edge:
                    return new SwEdge(edge);

                case IFeature feat:
                    return new SwFeature(model, feat, true);

                case IBody2 body:
                    return new SwBody(body);

                case IDisplayDimension dispDim:
                    return new SwDimension(dispDim);

                default:
                    return new SwObject(disp);
            }
        }

        public virtual object Dispatch { get; }

        internal SwObject(object dispatch)
        {
            Dispatch = dispatch;
        }

        public virtual bool IsSame(IXObject other)
        {
            if (other is SwObject)
            {
                return Dispatch == (other as SwObject).Dispatch;
            }
            else
            {
                return false;
            }
        }
    }
}