//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Runtime.InteropServices;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public class SwTempBody : SwBody, IDisposable
    {
        private IBody2 m_TempBody;

        public override IBody2 Body => m_TempBody;
        public override object Dispatch => m_TempBody;

        internal SwTempBody(IBody2 body) : base(null)
        {
            m_TempBody = ConvertToTempIfNeeded(body);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Marshal.ReleaseComObject(m_TempBody);
            }

            m_TempBody = null;
        }

        private IBody2 ConvertToTempIfNeeded(IBody2 body)
        {
            if (body.IsTemporaryBody())
            {
                return body;
            }
            else
            {
                return body.ICopy();
            }
        }
    }
}