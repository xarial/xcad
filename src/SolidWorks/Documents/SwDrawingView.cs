//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwDrawingView : IXDrawingView, ISwSelObject
    {
        IView DrawingView { get; }
    }

    public interface ISwModelBasedDrawingView : ISwDrawingView, IXModelViewBasedDrawingView
    {
    }

    public interface ISwProjectedDrawingView : ISwDrawingView, IXProjectedDrawingView
    {
    }

    public interface ISwAuxiliaryDrawingView : ISwDrawingView, IXAuxiliaryDrawingView
    {
    }

    public interface ISwSectionDrawingView : ISwDrawingView, IXSectionDrawingView
    {
    }

    public interface ISwDetailDrawingView : ISwDrawingView, IXDetailedDrawingView
    {
    }

    public interface ISwFlatPatternDrawingView : ISwDrawingView, IXFlatPatternDrawingView
    {
    }

    public interface ISwRelativeDrawingView : ISwDrawingView, IXRelativeDrawingView 
    {
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwDrawingView : SwSelObject, ISwDrawingView
    {
        protected readonly SwDrawing m_Drawing;

        public IView DrawingView => m_Creator.Element;

        protected readonly IElementCreator<IView> m_Creator;

        protected SwSheet Sheet 
        {
            get 
            {
                if (IsCommitted)
                {
                    return OwnerDocument.CreateObjectFromDispatch<SwSheet>(DrawingView.Sheet);
                }
                else 
                {
                    return m_ParentSheet;
                }
            }
        }

        internal SwDrawingView(IView drwView, SwDrawing drw)
            : this(drwView, drw, null)
        {
        }

        public override object Dispatch => DrawingView;

        private SwSheet m_ParentSheet;

        protected SwDrawingView(IView drwView, SwDrawing drw, SwSheet sheet)
            : base(drwView, drw, drw?.OwnerApplication)
        {
            m_Drawing = drw;
            m_Creator = new ElementCreator<IView>(CreateDrawingViewElement, drwView, drwView != null);

            m_ParentSheet = sheet;

            Dimensions = new SwDrawingViewDimensionRepository(drw, this);
            Annotations = new SwDrawingViewAnnotationCollection(this);
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        public override bool IsCommitted => m_Creator.IsCreated;

        internal override void Select(bool append, ISelectData selData) => SelectView(DrawingView, append, selData);

        internal void SetParentSheet(SwSheet parentSheet) 
        {
            m_ParentSheet = parentSheet;
        }

        private IView CreateDrawingViewElement(CancellationToken cancellationToken)
        {
            using (var sheetActivator = new SheetActivator(Sheet)) 
            {
                var drwView = CreateDrawingView(cancellationToken);

                if (drwView == null)
                {
                    throw new Exception("Failed to create drawing view");
                }

                if (!string.IsNullOrEmpty(Name))
                {
                    drwView.SetName2(Name);
                }

                var refConf = ReferencedConfiguration;

                if (refConf != null) 
                {
                    drwView.ReferencedConfiguration = refConf.Name;
                }

                var loc = Location;

                if (loc != null) 
                {
                    drwView.Position = new double[] { loc.X, loc.Y };
                }

                var scale = Scale;

                if (scale != null)
                {
                    drwView.ScaleRatio = new double[] { scale.Numerator, scale.Denominator };
                }

                if (Bodies != null) 
                {
                    drwView.Bodies = Bodies.Cast<ISwBody>().Select(b => b.Body).ToArray();
                }

                return drwView;
            }
        }

        protected virtual IView CreateDrawingView(CancellationToken cancellationToken)
            => throw new NotSupportedException("Creation of this drawing view is not supported");

        private void SelectView(IView view, bool append, ISelectData selData)
        {
            const string DRW_VIEW_TYPE_NAME = "DRAWINGVIEW";

            if (!OwnerModelDoc.Extension.SelectByID2(view.Name, DRW_VIEW_TYPE_NAME, 0, 0, 0, append, selData?.Mark ?? 0, selData?.Callout, 0))
            {
                throw new Exception("Failed to select drawing view");
            }
        }

        protected void RefreshView(IView view) 
        {
            SelectView(view, false, null);
            m_Drawing.Drawing.SuppressView();
            SelectView(view, false, null);
            m_Drawing.Drawing.UnsuppressView();
        }

        public string Name
        {
            get
            {
                if (IsCommitted)
                {
                    return DrawingView.Name;
                }
                else
                {
                    return m_Creator.CachedProperties.Get<string>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    DrawingView.SetName2(value);
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public Point Location
        {
            get
            {
                if (IsCommitted)
                {
                    var pos = DrawingView.Position as double[];

                    return new Point(pos[0], pos[1], 0);
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
                    DrawingView.Position = new double[] { value.X, value.Y };
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public IXDocument3D ReferencedDocument
        {
            get
            {
                if (IsCommitted)
                {
                    var refDoc = DrawingView.ReferencedDocument;

                    if (refDoc != null)
                    {
                        return (IXDocument3D)((SwDocumentCollection)OwnerApplication.Documents)[refDoc];
                    }
                    else
                    {
                        var refDocPath = DrawingView.GetReferencedModelName();

                        if (!string.IsNullOrEmpty(refDocPath))
                        {
                            if (((SwDocumentCollection)OwnerApplication.Documents).TryFindExistingDocumentByPath(refDocPath, out SwDocument doc))
                            {
                                return (ISwDocument3D)doc;
                            }
                            else
                            {
                                return (ISwDocument3D)((SwDocumentCollection)OwnerApplication.Documents).PreCreateFromPath(refDocPath);
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<IXDocument3D>();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    var refDoc = ReferencedDocument;

                    if (refDoc == null)
                    {
                        InsertPredefinedView(value);
                    }
                    else 
                    {
                        ReplaceViewDocument(refDoc, value);
                    }
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        private void InsertPredefinedView(IXDocument3D doc)
        {
            Select(false);

            if (!m_Drawing.Drawing.InsertModelInPredefinedView(doc.Path))
            {
                throw new Exception("Failed to insert model into a predefined view");
            }
        }

        private void ReplaceViewDocument(IXDocument3D oldDoc, IXDocument3D newDoc)
        {
            throw new NotImplementedException("View replacement is not implemented");
        }

        public IXConfiguration ReferencedConfiguration
        {
            get
            {
                if (IsCommitted)
                {
                    var refConfName = DrawingView.ReferencedConfiguration;

                    if (!string.IsNullOrEmpty(refConfName))
                    {
                        return ReferencedDocument.Configurations.First(
                            c => string.Equals(c.Name, refConfName, StringComparison.CurrentCultureIgnoreCase));
                    }
                    else
                    {
                        return null;
                    }
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<IXConfiguration>();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    DrawingView.ReferencedConfiguration = value.Name;
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public Scale Scale
        {
            get
            {
                if (IsCommitted)
                {
                    var scale = (double[])DrawingView.ScaleRatio;
                    return new Scale(scale[0], scale[1]);
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<Scale>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    DrawingView.ScaleRatio = new double[] { value.Numerator, value.Denominator };
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public Rect2D Boundary
        {
            get
            {
                var outline = (double[])DrawingView.GetOutline();
                var width = outline[2] - outline[0];
                var height = outline[3] - outline[1];

                var centerPt = new Point((outline[0] + outline[2]) / 2, (outline[1] + outline[3]) / 2, 0);

                return new Rect2D(width, height, centerPt);
            }
        }

        public Thickness Padding
        {
            get
            {
                const double VIEW_BORDER_RATIO = 0.02;

                var width = -1d;
                var height = -1d;

                DrawingView.Sheet.GetSize(ref width, ref height);

                var minSize = Math.Min(width, height);

                return new Thickness(minSize * VIEW_BORDER_RATIO);
            }
        }

        public IXDimensionRepository Dimensions { get; }
        
        public IXAnnotationRepository Annotations { get; }

        public IXDrawingView BaseView
        {
            get
            {
                if (!IsCommitted)
                {
                    return m_Creator.CachedProperties.Get<IXDrawingView>();
                }
                else
                {
                    var baseView = DrawingView.IGetBaseView();

                    if (baseView != null)
                    {
                        return m_Drawing.CreateObjectFromDispatch<ISwDrawingView>(baseView);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        public IEnumerable<IXDrawingView> DependentViews 
        {
            get 
            {
                var views = (object[])DrawingView.GetDependentViews(true, -1) ?? new object[0];

                return views.Select(v => m_Drawing.CreateObjectFromDispatch<ISwDrawingView>((IView)v));
            }
        }

        public IXBody[] Bodies 
        {
            get 
            {
                if (IsCommitted)
                {
                    return ((object[])DrawingView.Bodies ?? new object[0]).Select(b => OwnerDocument.CreateObjectFromDispatch<ISwBody>(b)).ToArray();
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<IXBody[]>();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    DrawingView.Bodies = value.Cast<ISwBody>().Select(b => b.Body).ToArray();
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public IXSketch2D Sketch => OwnerDocument.CreateObjectFromDispatch<ISwSketch2D>(DrawingView.GetSketch());

        public TransformMatrix Transformation => DrawingView.ModelToViewTransform.ToTransformMatrix();

        public IXEntityRepository VisibleEntities => new SwViewVisibleEntities(this);

        public ViewPolylineData[] Polylines 
        {
            get
            {
                var refDoc = (ISwDocument3D)ReferencedDocument;

                var viewTransform = Transformation;

                var sheetScale = Sheet.Scale.AsDouble();

                var scale = viewTransform.Scale.X / sheetScale;

                var transform = TransformMatrix.CreateFromTranslation(viewTransform.Translation.Scale(1 / sheetScale));

                var ents = (object[])DrawingView.GetPolylines7((short)swCrossHatchFilter_e.swCrossHatchExclude, out var polylinesDataObj);

                var res = new ViewPolylineData[ents.Length];

                var polylinesData = (double[])polylinesDataObj;

                int curEntIndex = 0;

                for (int i = 0; i < polylinesData.Length;)
                {
                    var shift = 0;

                    var type = (int)polylinesData[i + shift];
                    var geomDataSize = (int)polylinesData[i + ++shift];

                    if (geomDataSize > 0)
                    {
                        var geomData = polylinesData.Skip(i + shift + 1).Take(geomDataSize).ToArray();
                        shift += geomData.Length;
                    }

                    var lineColor = polylinesData[i + ++shift];
                    var lineStyle = polylinesData[i + ++shift];
                    var lineFont = polylinesData[i + ++shift];
                    var lineWeight = polylinesData[i + ++shift];
                    var layerId = (int)polylinesData[i + ++shift];
                    var layerOverride = (int)polylinesData[i + ++shift];
                    var polyPointsCount = (int)polylinesData[i + ++shift];

                    Point[] polyPoints;

                    if (polyPointsCount > 0)
                    {
                        polyPoints = new Point[polyPointsCount];

                        ++shift;

                        for (int j = 0; j < polyPointsCount; j++)
                        {
                            //NOTE: coordinate of the point is neither in 3D space nor in sheet space
                            //It represents the 3D scale coordinates but on the plane which is always XY
                            polyPoints[j] = new Point(
                                polylinesData[j * 3 + i + shift] * scale,
                                polylinesData[j * 3 + i + shift + 1] * scale,
                                0) * transform;
                        }

                        shift += polyPointsCount * 3;
                    }
                    else
                    {
                        polyPoints = new Point[0];
                    }

                    i += shift;

                    //NOTE: silhouette edges may be null
                    res[curEntIndex] = new ViewPolylineData(
                        ents[curEntIndex] != null ? refDoc.CreateObjectFromDispatch<ISwEntity>(ents[curEntIndex]) : null,
                        polyPoints);

                    curEntIndex++;
                }

                return res;
            }
        }

        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj) => ConvertObjectBoxed(obj) as TSelObject;

        public TSelObject ConvertObject<TSelObject>(TSelObject obj)
            where TSelObject : ISwSelObject
        {
            return (TSelObject)ConvertObjectBoxed(obj);
        }

        private ISwSelObject ConvertObjectBoxed(object obj)
        {
            if (obj is ISwSelObject)
            {
                if (OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2018))
                {
                    var disp = (obj as ISwSelObject).Dispatch;
                    var corrDisp = DrawingView.GetCorresponding(disp);

                    if (corrDisp != null)
                    {
                        return m_Drawing.CreateObjectFromDispatch<ISwSelObject>(corrDisp);
                    }
                    else
                    {
                        throw new Exception("Failed to convert the pointer of the object");
                    }
                }
                else
                {
                    throw new NotSupportedException("This API only available in SOLIDWORKS 2018 onwards");
                }
            }
            else
            {
                throw new InvalidCastException("Object is not SOLIDWORKS object");
            }
        }
    }

    internal static class SwDrawingViewExtension 
    {
        internal static void SelectFeature(this IView drwView, SwDrawing drw, IFeature feat, bool append) 
        {
            if (drw.OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2018))
            {
                var corrFeat = drwView.GetCorresponding(feat);
                
                var selData = drw.Model.ISelectionManager.CreateSelectData();
                
                selData.View = (View)drwView;

                if (!((IEntity)corrFeat).Select4(append, selData)) 
                {
                    throw new Exception("Failed to select feature in the drawing view");
                }
            }
            else 
            {
                var selSuffix = feat.GetNameForSelection(out string selType);

                var featComp = ((IEntity)feat).GetComponent();

                if (featComp != null)
                {
                    selSuffix = "/" + selSuffix.Substring(selSuffix.IndexOf('@') + 1);
                }
                else 
                {
                    selSuffix = "";
                }

                var selName = $"{feat.Name}@{drwView.RootDrawingComponent.Name}@{drwView.Name}" + selSuffix;

                if (!drw.Model.Extension.SelectByID2(selName, selType, 0, 0, 0, append, 0, null, (int)swSelectOption_e.swSelectOptionDefault))
                {
                    throw new Exception("Failed to select feature in the drawing view");
                }
            }
        }
    }

    internal class SwDrawingViewDimensionRepository : IXDimensionRepository
    {
        private readonly SwDrawing m_Drw;
        private readonly SwDrawingView m_View;
        
        internal SwDrawingViewDimensionRepository(SwDrawing drw, SwDrawingView view) 
        {
            m_Drw = drw;
            m_View = view;
        }

        public IXDimension this[string name] => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public void AddRange(IEnumerable<IXDimension> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public IEnumerator<IXDimension> GetEnumerator()
            => IterateDimensions().GetEnumerator();

        public T PreCreate<T>() where T : IXDimension
            => throw new NotImplementedException();

        public void RemoveRange(IEnumerable<IXDimension> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public bool TryGet(string name, out IXDimension ent)
            => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) => RepositoryHelper.FilterDefault(this, filters, reverseOrder);

        private IEnumerable<IXDimension> IterateDimensions() 
        {
            var dispDim = m_View.DrawingView.GetFirstDisplayDimension5();

            while (dispDim != null) 
            {
                yield return m_Drw.CreateObjectFromDispatch<ISwDimension>(dispDim);
                dispDim = dispDim.GetNext5();
            }
        }
    }

    internal class SwViewVisibleEntities : SwEntityRepository
    {
        private readonly SwDrawingView m_DrawingView;

        internal SwViewVisibleEntities(SwDrawingView drwView) 
        {
            m_DrawingView = drwView;
        }

        protected override IEnumerable<ISwEntity> IterateEntities(bool faces, bool edges, bool vertices, bool silhouetteEdges)
        {
            foreach (Component2 visComp in (object[])m_DrawingView.DrawingView.GetVisibleComponents() ?? new object[0])
            {
                if (faces) 
                {
                    foreach (var face in IterateSpecificEntities(visComp, swViewEntityType_e.swViewEntityType_Face)) 
                    {
                        yield return face;
                    }
                }

                if (edges)
                {
                    foreach (var edge in IterateSpecificEntities(visComp, swViewEntityType_e.swViewEntityType_Edge))
                    {
                        yield return edge;
                    }
                }

                if (vertices)
                {
                    foreach (var vertex in IterateSpecificEntities(visComp, swViewEntityType_e.swViewEntityType_Vertex))
                    {
                        yield return vertex;
                    }
                }

                if (silhouetteEdges)
                {
                    foreach (var silhouetteEdge in IterateSpecificEntities(visComp, swViewEntityType_e.swViewEntityType_SilhouetteEdge))
                    {
                        yield return silhouetteEdge;
                    }
                }
            }
        }

        private IEnumerable<ISwEntity> IterateSpecificEntities(Component2 visComp, swViewEntityType_e type) 
        {
            var refDoc = (ISwDocument3D)m_DrawingView.ReferencedDocument;

            //NOTE: silhouette edge is not an IEntity so cannot cast to IEntity in the for-each
            foreach (object visEnt in (object[])m_DrawingView.DrawingView.GetVisibleEntities2(visComp, (int)type) ?? new object[0])
            {
                yield return refDoc.CreateObjectFromDispatch<ISwEntity>(visEnt);
            }
        }
    }

    internal class SwModelBasedDrawingView : SwDrawingView, ISwModelBasedDrawingView
    {
        internal SwModelBasedDrawingView(IView drwView, SwDrawing drw)
            : base(drwView, drw)
        {
        }

        internal SwModelBasedDrawingView(SwDrawing drw, SwSheet sheet)
            : base(null, drw, sheet)
        {
        }

        protected override IView CreateDrawingView(CancellationToken cancellationToken)
        {   
            if (SourceModelView is SwNamedView)
            {
                var namedView = (SwNamedView)SourceModelView;

                var drwView = m_Drawing.Drawing.CreateDrawViewFromModelView3(
                    namedView.Owner.GetPathName(), namedView.Name, 0, 0, 0);

                return drwView;
            }
            else
            {
                throw new InvalidCastException("Only named views are supported");
            }
        }

        public IXModelView SourceModelView 
        {
            get 
            {
                if (!IsCommitted)
                {
                    return m_Creator.CachedProperties.Get<IXModelView>();
                }
                else 
                {
                    throw new NotSupportedException();
                }
            }
            set 
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }
    }

    internal class SwProjectedDrawingView : SwDrawingView, ISwProjectedDrawingView
    {
        internal SwProjectedDrawingView(IView drwView, SwDrawing drw) : base(drwView, drw)
        {
        }

        internal SwProjectedDrawingView(SwDrawing drw, SwSheet sheet)
            : base(null, drw, sheet)
        {
        }

        public ProjectedViewDirection_e Direction
        {
            get
            {
                if (!IsCommitted)
                {
                    return m_Creator.CachedProperties.Get<ProjectedViewDirection_e>();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        protected override IView CreateDrawingView(CancellationToken cancellationToken)
        {
            const double DEFAULT_OFFSET = 0.1;

            var offsetDir = CalculateViewOffsetDirection(Direction);

            BaseView.Select(false);

            var srcPos = (double[])((ISwDrawingView)BaseView).DrawingView.Position;

            var view = m_Drawing.Drawing.CreateUnfoldedViewAt3(
                            srcPos[0] + offsetDir.X * DEFAULT_OFFSET,
                            srcPos[1] + offsetDir.Y * DEFAULT_OFFSET,
                            0, false);

            if (view != null)
            {
                var srcViewOutline = (double[])((ISwDrawingView)BaseView).DrawingView.GetOutline();

                var srcWidth = srcViewOutline[2] - srcViewOutline[0];
                var srcHeight = srcViewOutline[3] - srcViewOutline[1];

                var destViewOutline = (double[])view.GetOutline();

                var destWidth = destViewOutline[2] - destViewOutline[0];
                var destHeight = destViewOutline[3] - destViewOutline[1];

                var offset = Math.Max(srcWidth, srcHeight) * 0.1;

                double margin;

                switch (Direction)
                {
                    case ProjectedViewDirection_e.Left:
                    case ProjectedViewDirection_e.Right:
                        margin = srcWidth / 2 + destWidth / 2 + offset;
                        break;

                    case ProjectedViewDirection_e.Top:
                    case ProjectedViewDirection_e.Bottom:
                        margin = srcHeight / 2 + destHeight / 2 + offset;
                        break;

                    case ProjectedViewDirection_e.IsoBottomLeft:
                    case ProjectedViewDirection_e.IsoBottomRight:
                    case ProjectedViewDirection_e.IsoTopLeft:
                    case ProjectedViewDirection_e.IsoTopRight:
                        margin = Math.Max(srcHeight, srcWidth) / 2 + Math.Max(destHeight, destWidth) / 2 + offset;
                        break;

                    default:
                        throw new NotSupportedException();
                }

                var newPos = new double[]
                {
                    srcPos[0] + offsetDir.X * margin,
                    srcPos[1] + offsetDir.Y * margin
                };

                view.Position = newPos;
            }

            return view;
        }

        private Vector CalculateViewOffsetDirection(ProjectedViewDirection_e projection)
        {
            double dirX;
            double dirY;

            switch (projection)
            {
                case ProjectedViewDirection_e.Left:
                    dirX = -1;
                    dirY = 0;
                    break;

                case ProjectedViewDirection_e.Top:
                    dirX = 0;
                    dirY = 1;
                    break;

                case ProjectedViewDirection_e.Right:
                    dirX = 1;
                    dirY = 0;
                    break;

                case ProjectedViewDirection_e.Bottom:
                    dirX = 0;
                    dirY = -1;
                    break;

                case ProjectedViewDirection_e.IsoTopLeft:
                    dirX = -1;
                    dirY = 1;
                    break;

                case ProjectedViewDirection_e.IsoTopRight:
                    dirX = 1;
                    dirY = 1;
                    break;

                case ProjectedViewDirection_e.IsoBottomLeft:
                    dirX = -1;
                    dirY = -1;
                    break;

                case ProjectedViewDirection_e.IsoBottomRight:
                    dirX = 1;
                    dirY = -1;
                    break;

                default:
                    throw new NotSupportedException($"'{projection}' is not supported");
            }

            return new Vector(dirX, dirY, 0);
        }
    }

    internal class SwAuxiliaryDrawingView : SwDrawingView, ISwAuxiliaryDrawingView
    {
        internal SwAuxiliaryDrawingView(IView drwView, SwDrawing drw) : base(drwView, drw)
        {
        }

        internal SwAuxiliaryDrawingView(SwDrawing drw, SwSheet sheet)
            : base(null, drw, sheet)
        {
        }
    }

    internal class SwSectionDrawingView : SwDrawingView, ISwSectionDrawingView
    {
        internal SwSectionDrawingView(IView drwView, SwDrawing drw) : base(drwView, drw)
        {
        }

        internal SwSectionDrawingView(SwDrawing drw, SwSheet sheet)
            : base(null, drw, sheet)
        {
        }

        public Line SectionLine
        {
            get
            {
                if (!IsCommitted)
                {
                    return m_Creator.CachedProperties.Get<Line>();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        protected override IView CreateDrawingView(CancellationToken cancellationToken)
        {
            var drwModel = m_Drawing.Model;
            var skMgr = drwModel.SketchManager;
            var mathUtils = OwnerApplication.Sw.IGetMathUtility();

            var srcView = ((ISwDrawingView)BaseView).DrawingView;

            if (m_Drawing.Drawing.ActivateView(srcView.Name))
            {
                var transform = srcView.ModelToViewTransform.IMultiply(srcView.IGetSketch().ModelToSketchTransform);

                var startPt = (IMathPoint)mathUtils.CreatePoint(new double[] { SectionLine.StartPoint.X, SectionLine.StartPoint.Y, SectionLine.StartPoint.Z });
                startPt = startPt.IMultiplyTransform(transform);

                var endPt = (IMathPoint)mathUtils.CreatePoint(new double[] { SectionLine.EndPoint.X, SectionLine.EndPoint.Y, SectionLine.EndPoint.Z });
                endPt = endPt.IMultiplyTransform(transform);

                var startCoord = (double[])startPt.ArrayData;
                var endCoord = (double[])endPt.ArrayData;

                var sectionLine = skMgr.CreateLine(startCoord[0], startCoord[1], startCoord[2], endCoord[0], endCoord[1], endCoord[2]);

                using (var selGrp = new SelectionGroup(m_Drawing, false))
                {
                    selGrp.Add(sectionLine);

                    var sectionView = m_Drawing.Drawing.CreateSectionViewAt5(0, 0, 0, "", (int)swCreateSectionViewAtOptions_e.swCreateSectionView_NotAligned, null, 0);
                    var section = sectionView.IGetSection();

                    section.SetAutoHatch(true);
                    section.SetDisplayOnlySurfaceCut(false);
                    section.SetPartialSection(false);
                    section.SetReversedCutDirection(false);
                    section.SetScaleWithModelChanges(true);
                    section.CutSurfaceBodies = false;
                    section.DisplaySurfaceBodies = false;
                    section.ExcludeSliceSectionBodies = false;

                    return sectionView;
                }
            }
            else 
            {
                throw new Exception("Failed to activate a view");
            }
        }
    }

    internal class SwDetailDrawingView : SwDrawingView, ISwDetailDrawingView
    {
        internal SwDetailDrawingView(IView drwView, SwDrawing drw) : base(drwView, drw)
        {
        }

        internal SwDetailDrawingView(SwDrawing drw, SwSheet sheet)
            : base(null, drw, sheet)
        {
        }

        public Circle DetailCircle
        {
            get
            {
                if (!IsCommitted)
                {
                    return m_Creator.CachedProperties.Get<Circle>();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        protected override IView CreateDrawingView(CancellationToken cancellationToken)
        {
            var drwModel = m_Drawing.Model;
            var skMgr = drwModel.SketchManager;
            var mathUtils = OwnerApplication.Sw.IGetMathUtility();

            var srcView = ((ISwDrawingView)BaseView).DrawingView;

            if (m_Drawing.Drawing.ActivateView(srcView.Name))
            {
                var transform = srcView.ModelToViewTransform.IMultiply(srcView.IGetSketch().ModelToSketchTransform);

                var centerMathPt = (IMathPoint)mathUtils.CreatePoint(new double[] { DetailCircle.CenterAxis.Point.X, DetailCircle.CenterAxis.Point.Y, DetailCircle.CenterAxis.Point.Z });
                centerMathPt = centerMathPt.IMultiplyTransform(transform);

                var centerCoord = (double[])centerMathPt.ArrayData;

                var circle = skMgr.CreateCircleByRadius(centerCoord[0], centerCoord[1], centerCoord[2], DetailCircle.Diameter / 2);

                using (var selGrp = new SelectionGroup(m_Drawing, false))
                {
                    selGrp.Add(circle);

                    var detailView = (IView)m_Drawing.Drawing.CreateDetailViewAt4(0, 0, 0, (int)swDetViewStyle_e.swDetViewSTANDARD, 1, 1,
                        "", (int)swDetCircleShowType_e.swDetCircleCIRCLE, true, true, false, 5);

                    var detailCircle = detailView.IGetDetail();

                    detailCircle.ScaleHatchPattern = true;

                    return detailView;
                }
            }
            else
            {
                throw new Exception($"Failed to activate '{srcView.Name}'");
            }
        }
    }

    internal class SwFlatPatternDrawingView : SwDrawingView, ISwFlatPatternDrawingView
    {
        /// <summary>
        /// This helper service allows to handle flat pattern creation of mutli-body sheet metal parts
        /// </summary>
        /// <remarks>SOLIDWORKS API does not support this directly</remarks>
        private class MultiBodyFlatPatternActivator : IDisposable
        {
            private readonly SwPart m_SheetMetalPart;

            private ISwConfiguration m_OrigConf;

            internal MultiBodyFlatPatternActivator(SwDrawing drw, SwSolidBody sheetMetalBody, SwPart sheetMetalPart, ref ISwConfiguration conf, ref string viewName)
            {
                m_SheetMetalPart = sheetMetalPart;
                m_OrigConf = m_SheetMetalPart.Configurations.Active;

                if (sheetMetalBody.Component != null) 
                {
                    sheetMetalBody = sheetMetalPart.ConvertObject<SwSolidBody>(sheetMetalBody);
                }

                //creating a temp view to generate a sheet metal configuration
                var tempView = drw.Sheets.Active.DrawingViews.PreCreate<ISwModelBasedDrawingView>();
                tempView.Bodies = new IXBody[] { sheetMetalBody };
                tempView.SourceModelView = ((IXModelView3DRepository)sheetMetalPart.ModelViews)[StandardViewType_e.Front];
                tempView.ReferencedConfiguration = conf;
                tempView.Commit();

                tempView.Select(false);

                //generating sheet metal configuration
                if (drw.Drawing.ChangeRefConfigurationOfFlatPatternView(sheetMetalPart.Path, (conf ?? sheetMetalPart.Configurations.Active).Name))
                {
                    conf = (ISwConfiguration)tempView.ReferencedConfiguration;

                    m_SheetMetalPart.Configurations.Active = conf;

                    //remembering view name, it is important as the temp view will be deleted and flat pattern view
                    //needs to be renamed to the temp view name and this will reset the default view counter
                    viewName = tempView.Name;

                    tempView.Delete();
                }
                else 
                {
                    throw new Exception("Failed to create a flat pattern view configuration");
                }
            }

            public void Dispose()
            {
                m_SheetMetalPart.Configurations.Active = m_OrigConf;
            }
        }

        internal SwFlatPatternDrawingView(IView drwView, SwDrawing drw) : base(drwView, drw)
        {
        }

        internal SwFlatPatternDrawingView(SwDrawing drw, SwSheet sheet)
            : base(null, drw, sheet)
        {
            m_Creator.CachedProperties.Set<FlatPatternViewOptions_e>(
                FlatPatternViewOptions_e.BendLines | FlatPatternViewOptions_e.BendNotes, nameof(Options));
        }

        public IXSolidBody SheetMetalBody
        {
            get
            {
                if (!IsCommitted)
                {
                    return m_Creator.CachedProperties.Get<IXSolidBody>();
                }
                else
                {
                    var face = GetFlatPatternFace(DrawingView);
                    return OwnerDocument.CreateObjectFromDispatch<ISwSolidBody>(face.IGetBody());
                }
            }
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        public FlatPatternViewOptions_e Options 
        {
            get 
            {
                if (IsCommitted)
                {
                    var opts = FlatPatternViewOptions_e.None;

                    if (DrawingView.ShowSheetMetalBendNotes) 
                    {
                        opts |= FlatPatternViewOptions_e.BendNotes;
                    }

                    if (DrawingView.GetBendLineCount() > 0) 
                    {
                        opts |= FlatPatternViewOptions_e.BendLines;
                    }

                    return opts;
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<FlatPatternViewOptions_e>();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    SetViewOptions(DrawingView, value);
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        protected override IView CreateDrawingView(CancellationToken cancellationToken)
        {
            var sheetMetalBody = (SwSolidBody)SheetMetalBody;

            SwPart sheetMetalPart;

            if (sheetMetalBody != null)
            {
                if (!sheetMetalBody.Body.IsSheetMetal())
                {
                    throw new InvalidCastException("Specified body is not a sheet metal");
                }

                if (sheetMetalBody.OwnerDocument is SwPart)
                {
                    sheetMetalPart = (SwPart)sheetMetalBody.OwnerDocument;
                }
                else 
                {
                    //NOTE: by some reasons GetComponent for the body returns null, otherwise it should be the component's reference document
                    sheetMetalPart = (SwPart)ReferencedDocument;
                }
            }
            else 
            {
                sheetMetalPart = (SwPart)ReferencedDocument;
            }

            if (sheetMetalPart == null) 
            {
                throw new NotSupportedException($"Sheet metal part is not specified. Either set the {nameof(SheetMetalBody)} or {nameof(ReferencedDocument)}");
            }

            if (!sheetMetalPart.IsCommitted)
            {
                if (!sheetMetalPart.State.HasFlag(DocumentState_e.Hidden)) 
                {
                    sheetMetalPart.State |= DocumentState_e.Hidden;
                }

                sheetMetalPart.Commit(cancellationToken);
            }

            var sheetMetalBodiesCount = sheetMetalPart.Bodies.Count(b => ((ISwBody)b).Body.IsSheetMetal());

            if (sheetMetalBodiesCount > 0)
            {
                if (sheetMetalBodiesCount > 1 && sheetMetalBody == null) 
                {
                    throw new Exception($"Set the body to create flat pattern for via {nameof(SheetMetalBody)} for multi body sheet metal part");
                }

                var refConf = (ISwConfiguration)ReferencedConfiguration;
                var viewName = "";

                using (var multiBodyFlatPatternActivator = 
                    sheetMetalBodiesCount > 1 
                    ? new MultiBodyFlatPatternActivator(m_Drawing, sheetMetalBody, sheetMetalPart, ref refConf, ref viewName) 
                    : null)
                {
                    return CreateFlatPatternView(sheetMetalPart, refConf, viewName);
                }
            }
            else 
            {
                throw new Exception("No sheet metal bodies found in the part");
            }
        }

        private IView CreateFlatPatternView(SwPart sheetMetalPart, ISwConfiguration refConf, string viewName)
        {
            var confName = "";

            if (refConf != null) 
            {
                confName = refConf.Name;
            }

            var view = m_Drawing.Drawing.CreateFlatPatternViewFromModelView3(sheetMetalPart.Path, confName, 0, 0, 0, false, false);

            if (view != null) 
            {
                if (!string.IsNullOrEmpty(viewName)) 
                {
                    view.SetName2(viewName);
                }

                if (!view.IsFlatPatternView()) 
                {
                    throw new Exception("Created view cannot be set to flat pattern");
                }

                //In some cases view shows invalid geometry until hidden and shown
                RefreshView(view);

                SetViewOptions(view, Options);

                if (ReferencedConfiguration != null) 
                {
                    //NOTE: flat pattern view creates sub configuration based on the active configuration and it was activated already while creation of the view
                    ReferencedConfiguration = null;
                }
            }

            return view;
        }

        private void SetViewOptions(IView view, FlatPatternViewOptions_e opts) 
        {
            var hasBendLines = view.GetBendLineCount() > 0;

            var needBendLines = opts.HasFlag(FlatPatternViewOptions_e.BendLines);

            //NOTE: if hiding bend lines bend notes must be hidden first
            if (!needBendLines)
            {
                view.ShowSheetMetalBendNotes = opts.HasFlag(FlatPatternViewOptions_e.BendNotes);

                if (view.ShowSheetMetalBendNotes) 
                {
                    throw new Exception("Bend notes cannot be displayed if bend lines are hidden");
                }
            }

            if (hasBendLines != needBendLines)
            {
                var bendLinesSketch = GetBendLinesSketchOrNull(view);

                if (bendLinesSketch != null) //bend line sketch can be null if sheet metal has no bends
                {
                    view.SelectFeature((SwDrawing)OwnerDocument, bendLinesSketch, false);

                    if (needBendLines)
                    {
                        OwnerDocument.Model.UnblankSketch();
                    }
                    else
                    {
                        OwnerDocument.Model.BlankSketch();
                    }
                }
            }

            //NOTE: if showing bend lines bend notes must be shown last
            if (needBendLines)
            {
                view.ShowSheetMetalBendNotes = opts.HasFlag(FlatPatternViewOptions_e.BendNotes);
            }
        }

        private IFeature GetBendLinesSketchOrNull(IView view)
        {
            var bendLines = (object[])view.GetBendLines();

            if (bendLines?.Any() == true)
            {
                return (IFeature)((ISketchSegment)bendLines.First()).GetSketch();
            }
            else
            {
                var flatPattern = GetViewFlatPattern(view);

                var subFeat = flatPattern.Feature.IGetFirstSubFeature();

                IFeature bendLinesSketch = null;

                while (subFeat != null)
                {
                    if (subFeat.GetTypeName2() == SwSketch2D.TypeName)
                    {
                        var sketch = (ISketch)subFeat.GetSpecificFeature2();
                        var skSegs = (object[])sketch.GetSketchSegments();

                        if ((skSegs?.FirstOrDefault() as ISketchSegment)?.IsBendLine() == true)
                        {
                            bendLinesSketch = subFeat;
                            break;
                        }
                    }

                    subFeat = subFeat.IGetNextSubFeature();
                }

                return bendLinesSketch;
            }
        }

        private ISwFlatPattern GetViewFlatPattern(IView view) 
        {
            if (OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2014))
            {
                var flatPatternFolder = (IFlatPatternFolder)view.ReferencedDocument.FeatureManager.GetFlatPatternFolder();
                var flatPatterns = (object[])flatPatternFolder.GetFlatPatterns();

                if (flatPatterns?.Any() == true)
                {
                    var activeFlatPatterns = flatPatterns.Cast<IFeature>().Where(f =>
                    {
                        var isSuppressed = ((bool[])f.IsSuppressed2((int)swInConfigurationOpts_e.swSpecifyConfiguration,
                            new string[] { view.ReferencedConfiguration })).First();

                        return !isSuppressed;
                    }).ToArray();

                    if (activeFlatPatterns.Length == 1)
                    {
                        return OwnerDocument.CreateObjectFromDispatch<ISwFlatPattern>(activeFlatPatterns.First());
                    }
                    else if (activeFlatPatterns.Length == 0)
                    {
                        throw new Exception("Failed to find active flat patterns");
                    }
                    else 
                    {
                        throw new Exception("More than one active flat pattern is found");
                    }
                }
                else 
                {
                    throw new Exception("No flat patterns found");
                }
            }
            else
            {
                //Note, in some sheet metal files (probably corrupted as the result of the upgrade)
                //this can return the hidden sheet metal flat pattern feature, not the actual one,
                //so only using this as a fallback function

                var face = GetFlatPatternFace(view);

                return OwnerDocument.CreateObjectFromDispatch<ISwFlatPattern>(face.GetFeature());
            }
        }

        private IFace2 GetFlatPatternFace(IView view)
        {
            var comp = (Component2)((object[])view.GetVisibleComponents()).First();

            IFace2 face;

            if (OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2014))
            {
                face = (IFace2)((object[])view.GetVisibleEntities2(comp, (int)swViewEntityType_e.swViewEntityType_Face)).First();
            }
            else
            {
                face = (IFace2)((object[])view.GetVisibleEntities(comp, (int)swViewEntityType_e.swViewEntityType_Face)).First();
            }

            return face;
        }
    }

    internal class SwRelativeView : SwDrawingView, ISwRelativeDrawingView
    {
        internal SwRelativeView(IView drwView, SwDrawing drw) : base(drwView, drw)
        {
        }

        internal SwRelativeView(SwDrawing drw, SwSheet sheet)
            : base(null, drw, sheet)
        {
        }

        public RelativeDrawingViewOrientation Orientation 
        {
            get 
            {
                if (!IsCommitted)
                {
                    return m_Creator.CachedProperties.Get<RelativeDrawingViewOrientation>();
                }
                else
                {
                    throw new Exception("This property is only available for new view creation");
                }
            }
            set 
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        protected override IView CreateDrawingView(CancellationToken cancellationToken)
        {
            swRelativeViewCreationDirection_e ConvertDirection(StandardViewType_e dir) 
            {
                switch (dir) 
                {
                    case StandardViewType_e.Front:
                        return swRelativeViewCreationDirection_e.swRelativeViewCreationDirection_FRONT;

                    case StandardViewType_e.Back:
                        return swRelativeViewCreationDirection_e.swRelativeViewCreationDirection_BACK;

                    case StandardViewType_e.Left:
                        return swRelativeViewCreationDirection_e.swRelativeViewCreationDirection_LEFT;

                    case StandardViewType_e.Right:
                        return swRelativeViewCreationDirection_e.swRelativeViewCreationDirection_RIGHT;

                    case StandardViewType_e.Top:
                        return swRelativeViewCreationDirection_e.swRelativeViewCreationDirection_TOP;

                    case StandardViewType_e.Bottom:
                        return swRelativeViewCreationDirection_e.swRelativeViewCreationDirection_BOTTOM;

                    default:
                        throw new NotSupportedException();
                }
            }

            if (Orientation == null) 
            {
                throw new Exception("Orientation is not specified");
            }

            if (Orientation.FirstEntity is SwSelObject && Orientation.SecondEntity is SwSelObject)
            {
                var refDoc = ((SwSelObject)Orientation.FirstEntity).OwnerDocument;
                
                var selData = refDoc.Model.ISelectionManager.CreateSelectData();

                selData.Mark = 1;

                ((SwSelObject)Orientation.FirstEntity).Select(false, selData);

                selData.Mark = 2;

                ((SwSelObject)Orientation.SecondEntity).Select(true, selData);

                var dirFront = ConvertDirection(Orientation.FirstDirection);
                var dirRight = ConvertDirection(Orientation.SecondDirection);

                var view = m_Drawing.Drawing.CreateRelativeView(refDoc.Path, Location?.X ?? 0, Location?.Y ?? 0, (int)dirFront, (int)dirRight);

                refDoc.Model.ClearSelection2(true);

                return view;
            }
            else
            {
                throw new NotSupportedException("Entities must be selection objects");
            }
        }
    }
}
