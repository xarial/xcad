using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Features.Delegates;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    internal class DrawingViewCreatedEventsHandler : SwModelEventsHandler<DrawingViewCreatedDelegate>
    {
        private readonly SwDrawing m_Drawing;
        private readonly ISwSheet m_Sheet;

        public DrawingViewCreatedEventsHandler(ISwSheet sheet, SwDrawing drw, ISwApplication app) : base(drw, app)
        {
            m_Drawing = drw;
            m_Sheet = sheet;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.AddItemNotify += OnAddItemNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.AddItemNotify -= OnAddItemNotify;
        }

        private int OnAddItemNotify(int entityType, string itemName)
        {
            if (entityType == (int)swNotifyEntityType_e.swNotifyDrawingView)
            {
                var viewFeat = m_Drawing.Features[itemName];

                var swView = (IView)viewFeat.Feature.GetSpecificFeature2();

                var sheet = swView.Sheet;

                if (m_App.Sw.IsSame(m_Sheet.Sheet, sheet) == (int)swObjectEquality.swObjectSame)
                {
                    Delegate?.Invoke(m_Drawing, m_Sheet, m_Doc.CreateObjectFromDispatch<ISwDrawingView>(swView));
                }
            }

            return HResult.S_OK;
        }
    }
}
