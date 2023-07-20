//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal class SheetActivator : IDisposable
    {
        private readonly ISldWorks m_App;
        private readonly IDrawingDoc m_Draw;
        private readonly ISheet m_CurrentSheet;

        internal SheetActivator(SwSheet sheet)
        {
            m_App = sheet.OwnerApplication.Sw;
            m_Draw = (IDrawingDoc)sheet.OwnerDocument.Model;

            m_CurrentSheet = (ISheet)m_Draw.GetCurrentSheet();

            if (m_App.IsSame(m_CurrentSheet, sheet.Sheet) != (int)swObjectEquality.swObjectSame)
            {
                if (!m_Draw.ActivateSheet(sheet.Name))
                {
                    throw new Exception($"Failed to activate '{sheet.Name}'");
                }
            }
        }

        public virtual void Dispose()
        {
            var activeSheet = (ISheet)m_Draw.GetCurrentSheet();

            if (m_App.IsSame(m_CurrentSheet, activeSheet) != (int)swObjectEquality.swObjectSame)
            {
                m_Draw.ActivateSheet(m_CurrentSheet.GetName());
            }
        }
    }
}
