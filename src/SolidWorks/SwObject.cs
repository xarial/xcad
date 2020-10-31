//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Sketch;

namespace Xarial.XCad.SolidWorks
{
    /// <inheritdoc/>
    public class SwObject : IXObject
    {
        public static TObj FromDispatch<TObj>(object disp)
            where TObj : SwObject
        {
            return (TObj)FromDispatch(disp, null);
        }

        public static SwObject FromDispatch(object disp)
        {
            return FromDispatch(disp, null);
        }

        public static TObj FromDispatch<TObj>(object disp, SwDocument doc)
            where TObj : SwObject
        {
            return (TObj)FromDispatch(disp, doc);
        }

        public static SwObject FromDispatch(object disp, SwDocument doc)
        {
            return FromDispatch(disp, doc, d => new SwObject(d));
        }

        internal static SwObject FromDispatch(object disp, SwDocument doc, Func<object, SwObject> defaultHandler)
        {
            switch (disp)
            {
                case IEdge edge:
                    var edgeCurve = edge.IGetCurve();
                    if (edgeCurve.IsCircle())
                    {
                        return new SwCircularEdge(edge);
                    }
                    else if (edgeCurve.IsLine())
                    {
                        return new SwLinearEdge(edge);
                    }
                    else
                    {
                        return new SwEdge(edge);
                    }

                case IFace2 face:
                    var faceSurf = face.IGetSurface();
                    if (faceSurf.IsPlane())
                    {
                        return new SwPlanarFace(face);
                    }
                    else if (faceSurf.IsCylinder())
                    {
                        return new SwCylindricalFace(face);
                    }
                    else
                    {
                        return new SwFace(face);
                    }

                case IFeature feat:
                    switch (feat.GetTypeName()) 
                    {
                        case "ProfileFeature":
                            return new SwSketch2D(doc, feat, true);
                        case "3DProfileFeature":
                            return new SwSketch3D(doc, feat, true);
                        default:
                            return new SwFeature(doc, feat, true);
                    }

                case IBody2 body:
                    if (!body.IsTemporaryBody())
                    {
                        return new SwBody(body);
                    }
                    else 
                    {
                        return new SwTempBody(body);
                    }

                case ISketchLine skLine:
                    return new SwSketchLine(doc, skLine, true);

                case ISketchPoint skPt:
                    return new SwSketchPoint(doc, skPt, true);

                case IDisplayDimension dispDim:
                    return new SwDimension(doc.Model, dispDim);

                case IConfiguration conf:
                    return new SwConfiguration(doc, conf);

                case IComponent2 comp:
                    return new SwComponent(comp, (SwAssembly)doc);

                case ISheet sheet:
                    return new SwSheet((SwDrawing)doc, sheet);

                case IView view:
                    return new SwDrawingView(view, (SwDrawing)doc);

                case ICurve curve:
                    switch ((swCurveTypes_e)curve.Identity()) 
                    {
                        case swCurveTypes_e.LINE_TYPE:
                            return new SwLineCurve(doc.App.Sw.IGetModeler(), curve, true);
                        case swCurveTypes_e.CIRCLE_TYPE:
                            return new SwArcCurve(doc.App.Sw.IGetModeler(), curve, true);
                        default:
                            return new SwCurve(doc.App.Sw.IGetModeler(), curve, true);
                    }

                default:
                    return defaultHandler.Invoke(disp);
            }
        }

        public virtual object Dispatch { get; }

        internal SwObject(object dispatch)
        {
            Dispatch = dispatch;
        }

        public virtual bool IsSame(IXObject other)
        {
            if (object.ReferenceEquals(this, other)) 
            {
                return true;
            }

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