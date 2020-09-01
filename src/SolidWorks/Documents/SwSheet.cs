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
        private readonly IDrawingDoc m_Drawing;

        public string Name
        {
            get => m_Sheet.GetName();
            set 
            {
                m_Sheet.SetName(value);
            }
        }

        internal SwSheet(IDrawingDoc draw, ISheet sheet) : base(sheet)
        {
            m_Drawing = draw;
            m_Sheet = sheet;
        }
    }
}
