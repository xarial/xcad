//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;

namespace Xarial.XCad.SolidWorks.Documents
{
    public class SwSheet : SwObject, IXSheet
    {
        private readonly ISheet m_Sheet;
        private readonly SwDrawing m_Drawing;

        public string Name
        {
            get => m_Sheet.GetName();
            set 
            {
                m_Sheet.SetName(value);
            }
        }

        public IXDrawingViewRepository DrawingViews { get; }

        //TODO: implement creation of new sheets
        public bool IsCommitted => true;

        internal SwSheet(SwDrawing draw, ISheet sheet) : base(sheet)
        {
            m_Drawing = draw;
            m_Sheet = sheet;
            DrawingViews = new SwDrawingViewsCollection(draw, sheet);
        }
    }
}
