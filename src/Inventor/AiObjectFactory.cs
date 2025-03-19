using Inventor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Inventor.Documents;
using Xarial.XCad.Inventor.Features;
using Xarial.XCad.Inventor.Geometry;

namespace Xarial.XCad.Inventor
{
    /// <summary>
    /// Factory for xCAD objects
    /// </summary>
    internal static class AiObjectFactory
    {
        //TODO: replace all constructors with static method New
        internal static TObj FromDispatch<TObj>(object disp, AiDocument doc, AiApplication app)
            where TObj : IXObject
        {
            if (typeof(IAiSelObject).IsAssignableFrom(typeof(TObj)))
            {
                return (TObj)FromDispatch(disp, doc, app, d => new AiSelObject(disp, doc, app));
            }
            else
            {
                return (TObj)FromDispatch(disp, doc, app, d => new AiObject(disp, doc, app));
            }
        }

        private static IAiObject FromDispatch(object disp, AiDocument doc, AiApplication app, Func<object, IAiObject> defaultHandler)
        {
            if (disp == null)
            {
                throw new ArgumentException("Dispatch is null");
            }

            switch (disp)
            {
                case PartFeature feat:
                    return AiFeature.New(feat, doc, app);
                        
                case Property prp:
                    return AiProperty.New(prp, doc, app);

                case SurfaceBody body:
                    if (body.IsSolid)
                    {
                        if (body.ComponentDefinition is SheetMetalComponentDefinition)
                        {
                            return AiSheetMetalBody.New(body, (AiPart)doc, app);
                        }
                        else
                        {
                            return AiSolidBody.New(body, (AiPart)doc, app);
                        }
                    }
                    else 
                    {
                        return AiSheetBody.New(body, (AiPart)doc, app);
                    }

                default:
                    return defaultHandler.Invoke(disp);
            }
        }
    }
}
