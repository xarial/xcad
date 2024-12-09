//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Surfaces;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Features.CustomFeature;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Surfaces;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.Sketch;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks
{
    /// <summary>
    /// Factory for xCAD objects
    /// </summary>
    internal static class SwObjectFactory 
    {
        //TODO: replace all constructors with static method New
        internal static TObj FromDispatch<TObj>(object disp, SwDocument doc, SwApplication app)
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

        private static ISwObject FromDispatch(object disp, SwDocument doc, SwApplication app, Func<object, ISwObject> defaultHandler)
        {
            if (disp == null) 
            {
                throw new ArgumentException("Dispatch is null");
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
                    var faceSurfIdentity = (swSurfaceTypes_e)faceSurf.Identity();
                    switch (faceSurfIdentity)
                    {
                        case swSurfaceTypes_e.PLANE_TYPE:
                            return new SwPlanarFace(face, doc, app);

                        case swSurfaceTypes_e.CYLINDER_TYPE:
                            return new SwCylindricalFace(face, doc, app);

                        case swSurfaceTypes_e.CONE_TYPE:
                            return new SwConicalFace(face, doc, app);

                        case swSurfaceTypes_e.SPHERE_TYPE:
                            return new SwSphericalFace(face, doc, app);

                        case swSurfaceTypes_e.TORUS_TYPE:
                            return new SwToroidalFace(face, doc, app);

                        case swSurfaceTypes_e.BSURF_TYPE:
                            return new SwBFace(face, doc, app);

                        case swSurfaceTypes_e.BLEND_TYPE:
                            return new SwBlendFace(face, doc, app);

                        case swSurfaceTypes_e.OFFSET_TYPE:
                            return new SwOffsetFace(face, doc, app);

                        case swSurfaceTypes_e.EXTRU_TYPE:
                            return new SwExtrudedFace(face, doc, app);

                        case swSurfaceTypes_e.SREV_TYPE:
                            return new SwRevolvedFace(face, doc, app);

                        default:
                            throw new NotSupportedException($"Not supported face type: {faceSurfIdentity}");
                    }

                case IVertex vertex:
                    return new SwVertex(vertex, doc, app);

                case ISilhouetteEdge silhouetteEdge:
                    return new SwSilhouetteEdge(silhouetteEdge, doc, app);

                case ISketch sketch:
                    if (sketch.Is3D())
                    {
                        return new SwSketch3D(sketch, doc, app, true);
                    }
                    else
                    {
                        return new SwSketch2D(sketch, doc, app, true);
                    }

                case IBody2 body:

                    var bodyType = (swBodyType_e)body.GetType();
                    var isTemp = body.IsTemporaryBody();

                    switch (bodyType)
                    {
                        case swBodyType_e.swSheetBody:
                            
                            if (IsPlanarSheetBody(body))
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
                                if (body.IsSheetMetal())
                                {
                                    return new SwSheetMetalBody(body, doc, app);
                                }
                                else 
                                {
                                    return new SwSolidBody(body, doc, app);
                                }
                            }
                            else
                            {
                                return new SwTempSolidBody(body, app);
                            }

                        case swBodyType_e.swWireBody:
                            if (!isTemp)
                            {
                                return new SwWireBody(body, doc, app);
                            }
                            else
                            {
                                return new SwTempWireBody(body, app);
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

                case ISketchPicture skPict:
                    return new SwSketchPicture(skPict, doc, app, true);

                case IDisplayDimension dispDim:
                    return SwDimension.New(dispDim, doc, app);

                case IDimension dim:
                    return SwDimension.New(dim, doc, app);

                case INote note:
                    return SwNote.New(note, doc, app);

                case IDrSection section:
                    return new SwSectionLine(section, doc, app);

                case IBreakLine breakLine:
                    return new SwBreakLine(breakLine, doc, app);

                case IDetailCircle detailCircle:
                    return new SwDetailCircle(detailCircle, doc, app);

                case ITableAnnotation tableAnn:
                    switch ((swTableAnnotationType_e)tableAnn.Type) 
                    {
                        case swTableAnnotationType_e.swTableAnnotation_BillOfMaterials:
                            return SwBomTable.New(tableAnn, doc, app);
                        default:
                            return SwTable.New(tableAnn, doc, app);
                    }

                case IAnnotation ann:
                    switch ((swAnnotationType_e)ann.GetType())
                    {
                        case swAnnotationType_e.swDisplayDimension:
                        case swAnnotationType_e.swNote:
                        case swAnnotationType_e.swTableAnnotation:
                            return FromDispatch(ann.GetSpecificAnnotation(), doc, app, defaultHandler);
                        default:
                            return SwAnnotation.New(ann, doc, app);
                    }

                case ILayer layer:
                    return new SwLayer(layer, doc, app);

                case IConfiguration conf:
                    switch (doc)
                    {
                        case SwAssembly assm:
                            return new SwAssemblyConfiguration(conf, assm, app, true);

                        case SwPart part:
                            return new SwPartConfiguration(conf, part, app, true);

                        default:
                            throw new Exception("Owner document must be 3D document or assembly");
                    }

                case IComponent2 comp:
                    
                    var compRefModel = comp.GetModelDoc2();

                    if (compRefModel != null)
                    {
                        switch (compRefModel)
                        {
                            case IPartDoc _:
                                return new SwPartComponent(comp, (SwAssembly)doc, app);

                            case IAssemblyDoc _:
                                return new SwAssemblyComponent(comp, (SwAssembly)doc, app);

                            default:
                                throw new NotSupportedException($"Unrecognized component type of '{comp.Name2}'");
                        }
                    }
                    else
                    {
                        var compFilePath = comp.GetPathName();
                        var ext = Path.GetExtension(compFilePath);

                        switch (ext.ToLower())
                        {
                            case ".sldprt":
                                return new SwPartComponent(comp, (SwAssembly)doc, app);
                            case ".sldasm":
                                return new SwAssemblyComponent(comp, (SwAssembly)doc, app);
                            default:
                                throw new NotSupportedException($"Component '{comp.Name2}' file '{compFilePath}' is not recognized");
                        }
                    }

                case ISheet sheet:
                    return new SwSheet(sheet, (SwDrawing)doc, app);

                case IView view:
                    if (view.IsFlatPatternView())
                    {
                        return new SwFlatPatternDrawingView(view, (SwDrawing)doc);
                    }
                    else
                    {
                        switch ((swDrawingViewTypes_e)view.Type)
                        {
                            case swDrawingViewTypes_e.swDrawingProjectedView:
                                return new SwProjectedDrawingView(view, (SwDrawing)doc);
                            case swDrawingViewTypes_e.swDrawingNamedView:
                                return new SwModelBasedDrawingView(view, (SwDrawing)doc);
                            case swDrawingViewTypes_e.swDrawingAuxiliaryView:
                                return new SwAuxiliaryDrawingView(view, (SwDrawing)doc);
                            case swDrawingViewTypes_e.swDrawingSectionView:
                                return new SwSectionDrawingView(view, (SwDrawing)doc);
                            case swDrawingViewTypes_e.swDrawingDetailView:
                                return new SwDetailDrawingView(view, (SwDrawing)doc);
                            case swDrawingViewTypes_e.swDrawingRelativeView:
                                return new SwRelativeView(view, (SwDrawing)doc);
                            default:
                                return new SwDrawingView(view, (SwDrawing)doc);
                        }
                    }
                    
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

                case ILoop2 loop:
                    return new SwLoop(loop, doc, app);

                case ISurface surf:
                    var surfIdentity = (swSurfaceTypes_e)surf.Identity();
                    switch (surfIdentity)
                    {
                        case swSurfaceTypes_e.PLANE_TYPE:
                            return new SwPlanarSurface(surf, doc, app);

                        case swSurfaceTypes_e.CYLINDER_TYPE:
                            return new SwCylindricalSurface(surf, doc, app);

                        case swSurfaceTypes_e.CONE_TYPE:
                            return new SwConicalSurface(surf, doc, app);

                        case swSurfaceTypes_e.SPHERE_TYPE:
                            return new SwSphericalSurface(surf, doc, app);

                        case swSurfaceTypes_e.TORUS_TYPE:
                            return new SwToroidalSurface(surf, doc, app);

                        case swSurfaceTypes_e.BSURF_TYPE:
                            return new SwBSurface(surf, doc, app);

                        case swSurfaceTypes_e.BLEND_TYPE:
                            return new SwBlendXSurface(surf, doc, app);

                        case swSurfaceTypes_e.OFFSET_TYPE:
                            return new SwOffsetSurface(surf, doc, app);

                        case swSurfaceTypes_e.EXTRU_TYPE:
                            return new SwExtrudedSurface(surf, doc, app);

                        case swSurfaceTypes_e.SREV_TYPE:
                            return new SwRevolvedSurface(surf, doc, app);

                        default:
                            throw new NotSupportedException($"Not supported surface type: {surfIdentity}");
                    }

                case IModelView modelView:
                    return new SwModelView(modelView, doc, app);

                case ISketchBlockInstance skBlockInst:
                    return new SwSketchBlockInstance((IFeature)skBlockInst, doc, app, true);

                case ISketchBlockDefinition skBlockDef:
                    return new SwSketchBlockDefinition(GetSketchBlockDefinitionFeature(doc.Model, skBlockDef), doc, app, true);

                case ICutListItem cutListItem:
                    return new SwCutListItem(cutListItem, (SwDocument3D)doc, app, true);

                case IFeature feat:
                    switch (feat.GetTypeName())
                    {
                        case SwSketch2D.TypeName:
                            return new SwSketch2D(feat, doc, app, true);
                        case SwSketch3D.TypeName:
                            return new SwSketch3D(feat, doc, app, true);
                        case "CutListFolder":
                            return new SwCutListItem(feat, (SwDocument3D)doc, app, true);
                        case "CoordSys":
                            return new SwCoordinateSystem(feat, doc, app, true);
                        case SwOrigin.TypeName:
                            return new SwOrigin(feat, doc, app, true);
                        case SwPlane.TypeName:
                            return new SwPlane(feat, doc, app, true);
                        case SwFlatPattern.TypeName:
                            return new SwFlatPattern(feat, doc, app, true);
                        case "SketchBlockInst":
                            return new SwSketchBlockInstance(feat, doc, app, true);
                        case "SketchBlockDef":
                            return new SwSketchBlockDefinition(feat, doc, app, true);
                        case "SketchBitmap":
                            return new SwSketchPicture(feat, doc, app, true);
                        case "BaseBody":
                            return new SwDumbBody(feat, doc, app, true);
                        case "WeldMemberFeat":
                            return new SwStructuralMember(feat, doc, app, true);
                        case "MacroFeature":
                            if (TryGetParameterType(feat, app, out Type paramType))
                            {
                                return SwMacroFeature<object>.CreateSpecificInstance(feat, doc, app, paramType);
                            }
                            else
                            {
                                return new SwMacroFeature(feat, doc, app, true);
                            }
                        default:
                            return new SwFeature(feat, doc, app, true);
                    }

                default:
                    return defaultHandler.Invoke(disp);
            }
        }

        //NOTE: some of the sheet bodies will have more than single face
        private static bool IsPlanarSheetBody(IBody2 body) 
        {
            var face = body.IGetFirstFace();

            Vector normal = null;

            while (face != null) 
            {
                if (!face.IGetSurface().IsPlane()) 
                {
                    return false;
                }

                if(normal != null)
                {
                    if (!normal.IsParallel(new Vector((double[])face.Normal)))
                    {
                        return false;
                    }
                }

                var nextFace = face.IGetNextFace();

                if (nextFace != null) 
                {
                    if (normal == null)
                    {
                        normal = new Vector((double[])face.Normal);
                    }
                }

                face = nextFace;
            }

            return true;
        }

        private static bool TryGetParameterType(IFeature feat, SwApplication app, out Type paramType)
        {
            var featData = feat.GetDefinition() as IMacroFeatureData;

            var macroFeatTypeProvider = app.Services.GetService<IMacroFeatureTypeProvider>();
            var type = macroFeatTypeProvider.ProvideType(featData);

            if (type != null)
            {
                if (type.IsAssignableToGenericType(typeof(SwMacroFeatureDefinition<>)))
                {
                    paramType = type.GetArgumentsOfGenericType(typeof(SwMacroFeatureDefinition<>)).First();
                    return true;
                }
            }

            paramType = null;
            return false;
        }

        //NOTE: retrieving the pointer to the feature from the feature tree for the consistency as IFeature retrieved from ISketchBlockDefinition has a different pointer to IFeature in the tree
        private static IFeature GetSketchBlockDefinitionFeature(IModelDoc2 model, ISketchBlockDefinition skBlockDef)
        {
            var feat = (IFeature)skBlockDef;

            var comp = (IComponent2)((IEntity)feat).GetComponent();

            var name = feat.Name;

            IFeature corrFeat;

            if (comp == null)
            {
                switch (model)
                {
                    case IPartDoc part:
                        corrFeat = (IFeature)part.FeatureByName(name);
                        break;

                    case IAssemblyDoc assm:
                        corrFeat = (IFeature)assm.FeatureByName(name);
                        break;

                    case IDrawingDoc drw:
                        corrFeat = (IFeature)drw.FeatureByName(name);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                corrFeat = comp.FeatureByName(name);
            }

            if (corrFeat != null)
            {
                return corrFeat;
            }
            else
            {
                Debug.Assert(false, "Failed to find corresponding feature");
                return feat;
            }
        }
    }
}