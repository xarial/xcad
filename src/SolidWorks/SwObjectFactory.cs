//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Linq;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Features.CustomFeature;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Surfaces;
using Xarial.XCad.SolidWorks.Sketch;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks
{
    /// <summary>
    /// Factory for xCAD objects
    /// </summary>
    internal static class SwObjectFactory 
    {
        internal static TObj FromDispatch<TObj>(object disp, ISwDocument doc, ISwApplication app)
            where TObj : IXObject
        {
            if (typeof(ISwSelObject).IsAssignableFrom(typeof(TObj))) 
            {
                return (TObj)FromDispatch(disp, doc, app, d => new SwSelObject(disp, doc, app));
            }
            else
            {
                return (TObj)FromDispatch(disp, doc, app, d => new SwObject(disp, doc, app));
            }
        }

        private static ISwObject FromDispatch(object disp, ISwDocument doc, ISwApplication app, Func<object, ISwObject> defaultHandler)
        {
            if (disp == null) 
            {
                throw new ArgumentException("Dispatch is null");
            }

            if (disp is IEntity)
            {
                var safeEnt = (disp as IEntity).GetSafeEntity();
                if (safeEnt != null)
                {
                    disp = safeEnt;
                }
            }

            switch (disp)
            {
                case IEdge edge:
                    var edgeCurve = edge.IGetCurve();
                    if (edgeCurve.IsCircle())
                    {
                        return new SwCircularEdge(edge, doc, app);
                    }
                    else if (edgeCurve.IsLine())
                    {
                        return new SwLinearEdge(edge, doc, app);
                    }
                    else
                    {
                        return new SwEdge(edge, doc, app);
                    }

                case IFace2 face:
                    var faceSurf = face.IGetSurface();
                    if (faceSurf.IsPlane())
                    {
                        return new SwPlanarFace(face, doc, app);
                    }
                    else if (faceSurf.IsCylinder())
                    {
                        return new SwCylindricalFace(face, doc, app);
                    }
                    else
                    {
                        return new SwFace(face, doc, app);
                    }

                case IVertex vertex:
                    return new SwVertex(vertex, doc, app);

                case IFeature feat:
                    switch (feat.GetTypeName())
                    {
                        case "ProfileFeature":
                            return new SwSketch2D(feat, doc, app, true);
                        case "3DProfileFeature":
                            return new SwSketch3D(feat, doc, app, true);
                        case "CutListFolder":
                            return new SwCutListItem(feat, (ISwDocument3D)doc, app, true);
                        case "CoordSys":
                            return new SwCoordinateSystem(feat, doc, app, true);
                        case "RefPlane":
                            return new SwPlane(feat, doc, app, true);
                        case "MacroFeature":
                            if (TryGetParameterType(feat, out Type paramType))
                            {
                                return SwMacroFeature<object>.CreateSpecificInstance(feat, (SwDocument)doc, app, paramType);
                            }
                            else
                            {
                                return new SwMacroFeature(feat, (SwDocument)doc, app, true);
                            }
                        default:
                            return new SwFeature(feat, doc, app, true);
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
                                    return new SwPlanarSheetBody(body, doc, app);
                                }
                                else
                                {
                                    return new SwTempPlanarSheetBody(body, app);
                                }
                            }
                            else
                            {
                                if (!isTemp)
                                {
                                    return new SwSheetBody(body, doc, app);
                                }
                                else
                                {
                                    return new SwTempSheetBody(body, app);
                                }
                            }

                        case swBodyType_e.swSolidBody:
                            if (!isTemp)
                            {
                                return new SwSolidBody(body, doc, app);
                            }
                            else
                            {
                                return new SwTempSolidBody(body, app);
                            }

                        default:
                            throw new NotSupportedException();
                    }

                case ISketchSegment seg:
                    switch ((swSketchSegments_e)seg.GetType())
                    {
                        case swSketchSegments_e.swSketchARC:
                            var arc = (ISketchArc)seg;
                            const int CIRCLE = 1;
                            if (arc.IsCircle() == CIRCLE)
                            {
                                return new SwSketchCircle(arc, doc, app, true);
                            }
                            else 
                            {
                                return new SwSketchArc(arc, doc, app, true);
                            }
                        case swSketchSegments_e.swSketchELLIPSE:
                            return new SwSketchEllipse((ISketchEllipse)seg, doc, app, true);
                        case swSketchSegments_e.swSketchLINE:
                            return new SwSketchLine((ISketchLine)seg, doc, app, true);
                        case swSketchSegments_e.swSketchPARABOLA:
                            return new SwSketchParabola((ISketchParabola)seg, doc, app, true);
                        case swSketchSegments_e.swSketchSPLINE:
                            return new SwSketchSpline((ISketchSpline)seg, doc, app, true);
                        case swSketchSegments_e.swSketchTEXT:
                            return new SwSketchText((ISketchText)seg, doc, app, true);
                        default:
                            throw new NotSupportedException();
                    }

                case ISketchRegion skReg:
                    return new SwSketchRegion(skReg, doc, app);

                case ISketchPoint skPt:
                    return new SwSketchPoint(skPt, doc, app, true);

                case IDisplayDimension dispDim:
                    return new SwDimension(dispDim, doc, app);

                case INote note:
                    return new SwNote(note, doc, app);

                case IAnnotation ann:
                    switch ((swAnnotationType_e)ann.GetType())
                    {
                        case swAnnotationType_e.swDisplayDimension:
                            return new SwDimension((IDisplayDimension)ann.GetSpecificAnnotation(), doc, app);
                        case swAnnotationType_e.swNote:
                            return new SwNote((INote)ann.GetSpecificAnnotation(), doc, app);
                        default:
                            return defaultHandler.Invoke(ann);
                    }

                case IConfiguration conf:
                    switch (doc)
                    {
                        case SwAssembly assm:
                            return new SwAssemblyConfiguration(conf, assm, app, true);

                        case SwDocument3D doc3D:
                            return new SwConfiguration(conf, doc3D, app, true);

                        default:
                            throw new Exception("Owner document must be 3D document or assembly");
                    }

                case IComponent2 comp:
                    return new SwComponent(comp, (SwAssembly)doc, app);

                case ISheet sheet:
                    return new SwSheet(sheet, (SwDrawing)doc, app);

                case IView view:
                    return new SwDrawingView(view, (SwDrawing)doc);

                case ICurve curve:
                    switch ((swCurveTypes_e)curve.Identity())
                    {
                        case swCurveTypes_e.LINE_TYPE:
                            return new SwLineCurve(curve, doc, app, true);
                        case swCurveTypes_e.CIRCLE_TYPE:
                            curve.GetEndParams(out _, out _, out bool isClosed, out _);
                            if (isClosed)
                            {
                                return new SwCircleCurve(curve, doc, app, true);
                            }
                            else 
                            {
                                return new SwArcCurve(curve, doc, app, true);
                            }
                        case swCurveTypes_e.BCURVE_TYPE:
                            return new SwBCurve(curve, doc, app, true);
                        case swCurveTypes_e.ELLIPSE_TYPE:
                            return new SwEllipseCurve(curve, doc, app, true);
                        case swCurveTypes_e.SPCURVE_TYPE:
                            return new SwSplineCurve(curve, doc, app, true);
                        default:
                            return new SwCurve(curve, doc, app, true);
                    }

                case ISurface surf:
                    switch ((swSurfaceTypes_e)surf.Identity())
                    {
                        case swSurfaceTypes_e.PLANE_TYPE:
                            return new SwPlanarSurface(surf, doc, app);

                        case swSurfaceTypes_e.CYLINDER_TYPE:
                            return new SwCylindricalSurface(surf, doc, app);

                        default:
                            return new SwSurface(surf, doc, app);
                    }

                default:
                    return defaultHandler.Invoke(disp);
            }
        }

        private static bool TryGetParameterType(IFeature feat, out Type paramType)
        {
            var featData = feat.GetDefinition() as IMacroFeatureData;
            var progId = featData.GetProgId();

            if (!string.IsNullOrEmpty(progId))
            {
                var type = Type.GetTypeFromProgID(progId);

                if (type != null)
                {
                    if (type.IsAssignableToGenericType(typeof(SwMacroFeatureDefinition<>)))
                    {
                        paramType = type.GetArgumentsOfGenericType(typeof(SwMacroFeatureDefinition<>)).First();
                        return true;
                    }
                }
            }

            paramType = null;
            return false;
        }
    }
}