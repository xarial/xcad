using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Services;

namespace Xarial.XCad.SolidWorks.Documents
{
    public class SwDrawingView : SwSelObject, IXDrawingView
    {
        protected readonly SwDrawing m_Drawing;

        public IView DrawingView => m_Creator.Element;

        private readonly ElementCreator<IView> m_Creator;

        private readonly ISheet m_Sheet;

        internal SwDrawingView(IView drwView, SwDrawing drw) 
            : this(drwView, drw, null, true)
        {
        }

        internal SwDrawingView(IView drwView, SwDrawing drw, ISheet sheet, bool created) 
            : base(drw.Model, drwView)
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

        internal void Create()
        {
            m_Creator.Create();
        }

        public override bool IsCommitted => m_Creator.IsCreated;

        public override void Select(bool append)
        {
            const string DRW_VIEW_TYPE_NAME = "DRAWINGVIEW";

            if (!m_ModelDoc.Extension.SelectByID2(DrawingView.Name, DRW_VIEW_TYPE_NAME, 0, 0, 0, append, 0, null, 0)) 
            {
                throw new Exception("Failed to select drawing view");
            }
        }

        private IView CreateDrawingViewElement() 
        {
            var curSheet = m_Drawing.Drawing.GetCurrentSheet() as ISheet;

            try
            {
                m_Drawing.Drawing.ActivateSheet(m_Sheet.GetName());
                return CreateDrawingView();
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

        protected virtual IView CreateDrawingView() 
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

        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj) => ConvertObjectBoxed(obj) as TSelObject;

        public TSelObject ConvertObject<TSelObject>(TSelObject obj)
            where TSelObject : SwSelObject
        {
            return (TSelObject)ConvertObjectBoxed(obj);
        }

        private SwSelObject ConvertObjectBoxed(object obj)
        {
            if (obj is SwSelObject)
            {
                if (m_Drawing.App.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2018))
                {
                    var disp = (obj as SwSelObject).Dispatch;
                    var corrDisp = DrawingView.GetCorresponding(disp);

                    if (corrDisp != null)
                    {
                        return SwSelObject.FromDispatch(corrDisp, m_Drawing);
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

    public class SwModelBasedDrawingView : SwDrawingView, IXModelViewBasedDrawingView
    {
        private SwNamedView m_BaseModelView;

        internal SwModelBasedDrawingView(IView drwView, SwDrawing drw, ISheet sheet, bool created)
            : base(drwView, drw, sheet, created)
        {
        }

        protected override IView CreateDrawingView()
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

        public IXView View 
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
