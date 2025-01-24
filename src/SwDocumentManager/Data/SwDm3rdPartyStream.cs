//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using Xarial.XCad.Toolkit.Data;

namespace Xarial.XCad.SolidWorks.Data
{
    internal class SwDm3rdPartyStream : ComStream
    {
        private readonly ISwDMDocument19 m_Doc;
        private readonly string m_Name;
        internal bool Exists;

        internal SwDm3rdPartyStream(ISwDMDocument19 doc, string name, bool write) 
            : base(write, false)
        {
            m_Doc = doc;
            m_Name = name;
            Exists = false;

            try
            {
                var stream = (IStream)m_Doc.Get3rdPartyStorage(name, write);

                if (stream != null)
                {
                    Load(stream);
                    Seek(0, SeekOrigin.Begin);
                    Exists = true;
                }
                else
                {
                    Release();
                }
            }
            catch 
            {
                Release();
                throw;
            }
        }

        private bool Release()
            => m_Doc.Release3rdPartyStorage(m_Name);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (Exists)
            {
                if (!Release()) 
                {
                    throw new Exception("Failed to release 3rd party stream");
                }
            }
        }
    }
}
