//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using Xarial.XCad.Annotations.Delegates;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.SolidWorks.Annotations.EventHandlers
{
    internal class SwDimensionChangeEventsHandler : EventsHandler<DimensionValueChangedDelegate>
    {
        private readonly SwDimension m_Dim;
        private readonly ISwDocument m_Doc;

        public SwDimensionChangeEventsHandler(SwDimension dim, ISwDocument doc) : base()
        {
            m_Dim = dim;
            m_Doc = doc;
        }

        protected override void SubscribeEvents()
        {
            switch (m_Doc.Model)
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
            switch (m_Doc.Model)
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
            if (m_Dim.DisplayDimension == displayDim)
            {
                Delegate.Invoke(m_Dim, m_Dim.Value);
            }

            return HResult.S_OK;
        }
    }
}