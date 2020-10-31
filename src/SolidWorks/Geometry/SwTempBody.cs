//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public class SwTempBody : SwBody, IDisposable
    {
        private IBody2 m_TempBody;

        public override IBody2 Body => m_TempBody;
        public override object Dispatch => m_TempBody;

        internal SwTempBody(IBody2 body) : base(null)
        {
            if (!body.IsTemporaryBody()) 
            {
                throw new ArgumentException("Body is not temp");
            }

            m_TempBody = body;
        }

        public void Preview(SwPart part, Color color, bool selectable = false) 
        {
            var opts = selectable 
                ? swTempBodySelectOptions_e.swTempBodySelectable
                : swTempBodySelectOptions_e.swTempBodySelectOptionNone;
            
            Body.Display3(part.Model, ColorUtils.ToColorRef(color), (int)opts);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Marshal.FinalReleaseComObject(m_TempBody);
            }

            m_TempBody = null;
        }
    }
}