//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Services;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwDrawingView : IXDrawingView, ISwSelObject
    {
        IView DrawingView { get; }
    }

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

        internal SwDrawingView(IView drwView, SwDrawing drw, ISheet sheet, bool created) 
            : base(drwView, drw, drw?.Application)
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
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);
        
        public override bool IsCommitted => m_Creator.IsCreated;

        public override void Select(bool append)
        {
            const string DRW_VIEW_TYPE_NAME = "DRAWINGVIEW";

            if (!ModelDoc.Extension.SelectByID2(DrawingView.Name, DRW_VIEW_TYPE_NAME, 0, 0, 0, append, 0, null, 0)) 
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
                    return (IXDocument3D)((SwDocumentCollection)Application.Documents)[refDoc];
                }
                else 
                {
                    var refDocPath = DrawingView.GetReferencedModelName();

                    if (!string.IsNullOrEmpty(refDocPath))
                    {

                        if (((SwDocumentCollection)Application.Documents).TryFindExistingDocumentByPath(refDocPath, out SwDocument doc))
                        {
                            return (ISwDocument3D)doc;
                        }
                        else
                        {
                            return (ISwDocument3D)((SwDocumentCollection)Application.Documents).PreCreateFromPath(refDocPath);
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
                if (Application.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2018))
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

    public interface ISwModelBasedDrawingView : ISwDrawingView, IXModelViewBasedDrawingView 
    {
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
}
