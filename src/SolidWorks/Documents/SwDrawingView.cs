using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;

namespace Xarial.XCad.SolidWorks.Documents
{
    public class SwDrawingView : SwSelObject, IXDrawingView
    {
        private readonly SwDrawing m_Drawing;
        private readonly IView m_DrwView;

        internal SwDrawingView(IView drwView, SwDrawing drw) : base(drw.Model, drwView)
        {
            m_Drawing = drw;
            m_DrwView = drwView;
        }

        public string Name 
        {
            get => m_DrwView.Name;
            set => m_DrwView.SetName2(value);
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
                var disp = (obj as SwSelObject).Dispatch;
                var corrDisp = m_DrwView.GetCorresponding(disp);

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
                throw new InvalidCastException("Object is not SOLIDWORKS object");
            }
        }
    }
}
