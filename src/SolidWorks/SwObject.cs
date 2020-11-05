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
using Xarial.XCad.SolidWorks.Geometry.Surfaces;
using Xarial.XCad.SolidWorks.Sketch;

namespace Xarial.XCad.SolidWorks
{
    public interface ISwObject : IXObject
    {
        object Dispatch { get; }
    }

    /// <inheritdoc/>
    public class SwObject : ISwObject
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

        public static TObj FromDispatch<TObj>(object disp, ISwDocument doc)
            where TObj : SwObject
        {
            return (TObj)FromDispatch(disp, doc);
        }

        public static SwObject FromDispatch(object disp, ISwDocument doc)
        {
            return FromDispatch(disp, doc, d => new SwObject(d));
        }

        internal static SwObject FromDispatch(object disp, ISwDocument doc, Func<object, SwObject> defaultHandler)
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
                    
                    var bodyType = (swBodyType_e)body.GetType();
                    var isTemp = body.IsTemporaryBody();

                    switch (bodyType)
                    {
                        case swBodyType_e.swSheetBody:
                            if (body.GetFaceCount() == 1 && body.IGetFirstFace().IGetSurface().IsPlane())
                            {
                                if (!isTemp)
                                {
                                    return new SwPlanarSheetBody(body, doc);
                                }
                                else
                                {
                                    return new SwTempPlanarSheetBody(body);
                                }
                            }
                            else
                            {
                                if (!isTemp)
                                {
                                    return new SwSheetBody(body, doc);
                                }
                                else
                                {
                                    return new SwTempSheetBody(body);
                                }
                            }

                        case swBodyType_e.swSolidBody:
                            if (!isTemp)
                            {
                                return new SwSolidBody(body, doc);
                            }
                            else
                            {
                                return new SwTempSolidBody(body);
                            }

                        default:
                            throw new NotSupportedException();
                    }
                    

                case ISketchSegment seg:
                    switch ((swSketchSegments_e)seg.GetType()) 
                    {
                        case swSketchSegments_e.swSketchARC:
                            return new SwSketchArc(doc, seg as ISketchArc, true);
                        case swSketchSegments_e.swSketchELLIPSE:
                            return new SwSketchEllipse(doc, seg as ISketchEllipse, true);
                        case swSketchSegments_e.swSketchLINE:
                            return new SwSketchLine(doc, seg as ISketchLine, true);
                        case swSketchSegments_e.swSketchPARABOLA:
                            return new SwSketchParabola(doc, seg as ISketchParabola, true);
                        case swSketchSegments_e.swSketchSPLINE:
                            return new SwSketchSpline(doc, seg as ISketchSpline, true);
                        case swSketchSegments_e.swSketchTEXT:
                            return new SwSketchText(doc, seg as ISketchText, true);
                        default:
                            throw new NotSupportedException();
                    }
                
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
                            return new SwLineCurve(doc?.App.Sw.IGetModeler(), curve, true);
                        case swCurveTypes_e.CIRCLE_TYPE:
                            return new SwArcCurve(doc?.App.Sw.IGetModeler(), curve, true);
                        default:
                            return new SwCurve(doc?.App.Sw.IGetModeler(), curve, true);
                    }

                case ISurface surf:
                    switch ((swSurfaceTypes_e)surf.Identity()) 
                    {
                        case swSurfaceTypes_e.PLANE_TYPE:
                            return new SwPlanarSurface(surf);

                        case swSurfaceTypes_e.CYLINDER_TYPE:
                            return new SwCylindricalSurface(surf);

                        default:
                            return new SwSurface(surf);
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