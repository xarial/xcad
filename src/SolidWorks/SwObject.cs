//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.IO;
using System.Linq;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Exceptions;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Features.CustomFeature;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Surfaces;
using Xarial.XCad.SolidWorks.Sketch;
using Xarial.XCad.Toolkit.Data;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks
{
    /// <summary>
    /// Represents base interface for all SOLIDWORKS objects
    /// </summary>
    public interface ISwObject : IXObject
    {
        /// <summary>
        /// SOLIDWORKS specific dispatch
        /// </summary>
        object Dispatch { get; }
    }

    /// <inheritdoc/>
    internal class SwObject : ISwObject
    {
        protected IModelDoc2 ModelDoc => m_Doc.Model;

        internal ISwDocument Document => m_Doc;

        protected readonly ISwDocument m_Doc;

        internal static TObj FromDispatch<TObj>(object disp)
            where TObj : ISwObject
            => (TObj)FromDispatch(disp, null);

        internal static ISwObject FromDispatch(object disp)
            => FromDispatch(disp, null);

        internal static TObj FromDispatch<TObj>(object disp, ISwDocument doc)
            where TObj : ISwObject
            => (TObj)FromDispatch(disp, doc);

        internal static ISwObject FromDispatch(object disp, ISwDocument doc)
            => FromDispatch(disp, doc, d => new SwObject(d, doc));

        internal static ISwObject FromDispatch(object disp, ISwDocument doc, Func<object, ISwObject> defaultHandler)
        {
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
                        return new SwCircularEdge(edge, doc);
                    }
                    else if (edgeCurve.IsLine())
                    {
                        return new SwLinearEdge(edge, doc);
                    }
                    else
                    {
                        return new SwEdge(edge, doc);
                    }

                case IFace2 face:
                    var faceSurf = face.IGetSurface();
                    if (faceSurf.IsPlane())
                    {
                        return new SwPlanarFace(face, doc);
                    }
                    else if (faceSurf.IsCylinder())
                    {
                        return new SwCylindricalFace(face, doc);
                    }
                    else
                    {
                        return new SwFace(face, doc);
                    }

                case IFeature feat:
                    switch (feat.GetTypeName()) 
                    {
                        case "ProfileFeature":
                            return new SwSketch2D(doc, feat, true);
                        case "3DProfileFeature":
                            return new SwSketch3D(doc, feat, true);
                        case "CutListFolder":
                            return new SwCutListItem((ISwDocument3D)doc, feat, true);
                        case "CoordSys":
                            return new SwCoordinateSystem(doc, feat, true);
                        case "RefPlane":
                            return new SwPlane(doc, feat, true);
                        case "MacroFeature":
                            if (TryGetParameterType(feat, out Type paramType))
                            {
                                return SwMacroFeature<object>.CreateSpecificInstance((SwDocument)doc, feat, paramType);
                            }
                            else
                            {
                                return new SwMacroFeature((SwDocument)doc, doc.Model.FeatureManager, feat, true);
                            }
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

                case ISketchRegion skReg:
                    return new SwSketchRegion(skReg, doc);

                case ISketchPoint skPt:
                    return new SwSketchPoint(doc, skPt, true);

                case IDisplayDimension dispDim:
                    return new SwDimension(doc, dispDim);

                case INote note:
                    return new SwNote(note, doc);

                case IAnnotation ann:
                    switch ((swAnnotationType_e)ann.GetType()) 
                    {
                        case swAnnotationType_e.swDisplayDimension:
                            return new SwDimension(doc, (IDisplayDimension)ann.GetSpecificAnnotation());
                        case swAnnotationType_e.swNote:
                            return new SwNote((INote)ann.GetSpecificAnnotation(), doc);
                        default:
                            return defaultHandler.Invoke(ann);
                    }

                case IConfiguration conf:
                    switch (doc) 
                    {
                        case SwAssembly assm:
                            return new SwAssemblyConfiguration(assm, conf, true);

                        case SwDocument3D doc3D:
                            return new SwConfiguration(doc3D, conf, true);

                        default:
                            throw new Exception("Owner document must be 3D document or assembly");
                    }

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
                            return new SwLineCurve(((SwDocument)doc)?.App.Sw.IGetModeler(), curve, true);
                        case swCurveTypes_e.CIRCLE_TYPE:
                            return new SwArcCurve(((SwDocument)doc)?.App.Sw.IGetModeler(), curve, true);
                        default:
                            return new SwCurve(((SwDocument)doc)?.App.Sw.IGetModeler(), curve, true);
                    }

                case ISurface surf:
                    switch ((swSurfaceTypes_e)surf.Identity()) 
                    {
                        case swSurfaceTypes_e.PLANE_TYPE:
                            return new SwPlanarSurface(surf, doc);

                        case swSurfaceTypes_e.CYLINDER_TYPE:
                            return new SwCylindricalSurface(surf, doc);

                        default:
                            return new SwSurface(surf, doc);
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

        public virtual object Dispatch { get; }

        public virtual bool IsAlive 
        {
            get 
            {
                try
                {
                    if (Dispatch != null)
                    {
                        if (ModelDoc.Extension.GetPersistReference3(Dispatch) != null)
                        {
                            return true;
                        }
                    }
                }
                catch 
                {
                }

                return false;
            }
        }

        public ITagsManager Tags => m_TagsLazy.Value;

        private readonly Lazy<ITagsManager> m_TagsLazy;

        internal SwObject(object dispatch)
        {
            Dispatch = dispatch;
            m_TagsLazy = new Lazy<ITagsManager>(() => new TagsManager());
        }

        internal SwObject(object dispatch, ISwDocument doc) : this(dispatch)
        {
            m_Doc = doc;
        }

        public virtual bool Equals(IXObject other)
        {
            if (object.ReferenceEquals(this, other)) 
            {
                return true;
            }

            if (other is ISwObject)
            {
                if (this is IXTransaction && !((IXTransaction)this).IsCommitted) 
                {
                    return false;
                }

                if (other is IXTransaction && !((IXTransaction)other).IsCommitted)
                {
                    return false;
                }

                return Dispatch == (other as ISwObject).Dispatch;
            }
            else
            {
                return false;
            }
        }

        public virtual void Serialize(Stream stream)
        {
            if (ModelDoc != null)
            {
                var disp = Dispatch;

                if (disp != null)
                {
                    var persRef = ModelDoc.Extension.GetPersistReference3(disp) as byte[];

                    if (persRef == null)
                    {
                        throw new ObjectSerializationException("Failed to serialize the object", -1);
                    }

                    stream.Write(persRef, 0, persRef.Length);
                    return;
                }
            }
            else 
            {
                throw new ObjectSerializationException("Model is not set for this object", -1);
            }
        }
    }
}