//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Annotations;

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

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwDrawingView : SwSelObject, ISwDrawingView
    {
        protected readonly SwDrawing m_Drawing;

        public IView DrawingView => m_Creator.Element;

        private readonly ElementCreator<IView> m_Creator;

        private readonly ISheet m_Sheet;

        internal SwDrawingView(IView drwView, SwDrawing drw)
            : this(drwView, drw, null, true)
        {
        }

        public override object Dispatch => DrawingView;

        protected SwDrawingView(IView drwView, SwDrawing drw, ISheet sheet, bool created)
            : base(drwView, drw, drw?.OwnerApplication)
        {
            m_Drawing = drw;
            m_Creator = new ElementCreator<IView>(CreateDrawingView, drwView, created);

            if (created)
            {
                sheet = drwView.Sheet;
            }
            else
            {
                m_CachedLocation = new Point(0, 0, 0);
            }

            m_Sheet = sheet;

            Dimensions = new SwDrawingViewDimensionRepository(drw, this);
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        public override bool IsCommitted => m_Creator.IsCreated;

        public override void Select(bool append)
        {
            const string DRW_VIEW_TYPE_NAME = "DRAWINGVIEW";

            if (!OwnerModelDoc.Extension.SelectByID2(DrawingView.Name, DRW_VIEW_TYPE_NAME, 0, 0, 0, append, 0, null, 0))
            {
                throw new Exception("Failed to select drawing view");
            }
        }

        private IView CreateDrawingViewElement(CancellationToken cancellationToken)
        {
            var curSheet = m_Drawing.Drawing.GetCurrentSheet() as ISheet;

            try
            {
                m_Drawing.Drawing.ActivateSheet(m_Sheet.GetName());
                return CreateDrawingView(cancellationToken);
            }
            catch
            {
                throw;
            }
            finally
            {
                m_Drawing.Drawing.ActivateSheet(curSheet.GetName());
            }
        }

        protected virtual IView CreateDrawingView(CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Creation of this drawing view is not supported"); ;
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
                    return m_CachedName;
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
                    m_CachedName = value;
                }
            }
        }

        private Point m_CachedLocation;
        private string m_CachedName;

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
                    return m_CachedLocation;
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
                    m_CachedLocation = value;
                }
            }
        }

        public IXDocument3D ReferencedDocument
        {
            get
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
        }

        public IXConfiguration ReferencedConfiguration
        {
            get
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
        }

        public Scale Scale
        {
            get
            {
                var scale = (double[])DrawingView.ScaleRatio;
                return new Scale(scale[0], scale[1]);
            }
            set => DrawingView.ScaleRatio = new double[] { value.Numerator, value.Denominator };
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

        public IXDrawingView BaseView
        {
            get
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

        public IEnumerable<IXDrawingView> DependentViews 
        {
            get 
            {
                var views = (object[])DrawingView.GetDependentViews(true, -1) ?? new object[0];

                return views.Select(v => m_Drawing.CreateObjectFromDispatch<ISwDrawingView>((IView)v));
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

        public void AddRange(IEnumerable<IXDimension> ents)
            => throw new NotImplementedException();

        public IEnumerator<IXDimension> GetEnumerator()
            => IterateDimensions().GetEnumerator();

        public void RemoveRange(IEnumerable<IXDimension> ents)
            => throw new NotImplementedException();

        public bool TryGet(string name, out IXDimension ent)
            => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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

    internal class SwModelBasedDrawingView : SwDrawingView, ISwModelBasedDrawingView
    {
        private SwNamedView m_BaseModelView;

        internal SwModelBasedDrawingView(IView drwView, SwDrawing drw, ISheet sheet, bool created)
            : base(drwView, drw, sheet, created)
        {
        }

        protected override IView CreateDrawingView(CancellationToken cancellationToken)
        {
            var drwView = m_Drawing.Drawing.CreateDrawViewFromModelView3(
                m_BaseModelView.Owner.GetPathName(), m_BaseModelView.Name, Location.X, Location.Y, Location.Z);

            if (drwView == null)
            {
                throw new Exception("Failed to create drawing view");
            }

            drwView.SetName2(Name);

            return drwView;
        }

        public IXModelView SourceModelView 
        {
            get => m_BaseModelView;
            set 
            {
                if (IsCommitted) 
                {
                    throw new Exception("Cannot modify already created drawing view");
                }

                if (value is SwNamedView)
                {
                    m_BaseModelView = (SwNamedView)value;
                }
                else 
                {
                    throw new InvalidCastException("Only named views are supported");
                }
            }
        }
    }

    internal class SwProjectedDrawingView : SwDrawingView, ISwProjectedDrawingView
    {
        internal SwProjectedDrawingView(IView drwView, SwDrawing drw, ISheet sheet, bool created) : base(drwView, drw, sheet, created)
        {
        }
    }

    internal class SwAuxiliaryDrawingView : SwDrawingView, ISwAuxiliaryDrawingView
    {
        internal SwAuxiliaryDrawingView(IView drwView, SwDrawing drw, ISheet sheet, bool created) : base(drwView, drw, sheet, created)
        {
        }
    }

    internal class SwSectionDrawingView : SwDrawingView, ISwSectionDrawingView
    {
        internal SwSectionDrawingView(IView drwView, SwDrawing drw, ISheet sheet, bool created) : base(drwView, drw, sheet, created)
        {
        }
    }

    internal class SwDetailDrawingView : SwDrawingView, ISwDetailDrawingView
    {
        internal SwDetailDrawingView(IView drwView, SwDrawing drw, ISheet sheet, bool created) : base(drwView, drw, sheet, created)
        {
        }
    }
}
