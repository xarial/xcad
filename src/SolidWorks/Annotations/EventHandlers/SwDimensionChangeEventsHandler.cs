//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using Xarial.XCad.Annotations.Delegates;
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.SolidWorks.Annotations.EventHandlers
{
    internal class SwDimensionChangeEventsHandler : EventsHandler<DimensionValueChangedDelegate>
    {
        private readonly SwDimension m_Dim;
        private readonly IModelDoc2 m_Model;

        public SwDimensionChangeEventsHandler(SwDimension dim, IModelDoc2 model) : base()
        {
            m_Dim = dim;
            m_Model = model;
        }

        protected override void SubscribeEvents()
        {
            switch (m_Model)
            {
                case PartDoc part:
                    part.DimensionChangeNotify += OnDimensionChangeNotify;
                    break;

                case AssemblyDoc asm:
                    asm.DimensionChangeNotify += OnDimensionChangeNotify;
                    break;

                case DrawingDoc drw:
                    drw.DimensionChangeNotify += OnDimensionChangeNotify;
                    break;
            }
        }

        protected override void UnsubscribeEvents()
        {
            switch (m_Model)
            {
                case PartDoc part:
                    part.DimensionChangeNotify -= OnDimensionChangeNotify;
                    break;

                case AssemblyDoc asm:
                    asm.DimensionChangeNotify -= OnDimensionChangeNotify;
                    break;

                case DrawingDoc drw:
                    drw.DimensionChangeNotify -= OnDimensionChangeNotify;
                    break;
            }
        }

        private int OnDimensionChangeNotify(object displayDim)
        {
            Delegate.Invoke(m_Dim, m_Dim.GetValue());

            return S_OK;
        }
    }
}