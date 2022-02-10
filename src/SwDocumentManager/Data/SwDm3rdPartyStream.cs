//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Xarial.XCad.Data.Enums;
using Xarial.XCad.Toolkit.Data;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Data
{
    internal class SwDm3rdPartyStream : ComStream
    {
        private readonly ISwDMDocument19 m_Doc;
        private readonly string m_Name;
        private readonly bool m_IsActive;

        internal SwDm3rdPartyStream(ISwDMDocument19 doc, string name, AccessType_e access) 
            : base(AccessTypeHelper.GetIsWriting(access), false)
        {
            m_Doc = doc;
            m_Name = name;
            m_IsActive = false;

            try
            {
                var stream = m_Doc.Get3rdPartyStorage(name, AccessTypeHelper.GetIsWriting(access)) as IStream;

                if (stream != null)
                {
                    Load(stream);
                    m_IsActive = true;
                }
                else 
                {
                    throw new Exception("Stream doesn't exist");
                }
            }
            catch 
            {
                m_Doc.Release3rdPartyStorage(m_Name);
                throw;
            }

            Seek(0, SeekOrigin.Begin);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (m_IsActive)
            {
                m_Doc.Release3rdPartyStorage(m_Name);
            }
        }
    }
}
