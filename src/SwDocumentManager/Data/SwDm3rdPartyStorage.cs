//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using Xarial.XCad.Toolkit.Data;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Data
{
    internal class SwDm3rdPartyStorage : ComStorage
    {
        private readonly ISwDMDocument19 m_Doc;
        private readonly string m_Name;

        internal bool Exists { get; }

        internal SwDm3rdPartyStorage(ISwDMDocument19 doc, string name, bool write) 
            : base(write)
        {
            m_Doc = doc;
            m_Name = name;
            Exists = false;

            try
            {
                var storage = (IComStorage)m_Doc.Get3rdPartyStorageStore(name, write);

                if (storage != null)
                {
                    Load(storage);
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
            => m_Doc.Release3rdPartyStorageStore(m_Name);

        public override void Dispose()
        {
            base.Dispose();

            if (Exists)
            {
                if (!Release())
                {
                    throw new InvalidOperationException("Failed to release 3rd party storage store");
                }
            }
        }
    }
}
