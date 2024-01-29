//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using Xarial.XCad.Data.Enums;
using Xarial.XCad.Toolkit.Data;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Data
{
    internal class SwDm3rdPartyStorage : ComStorage
    {
        private readonly ISwDMDocument19 m_Doc;
        private readonly string m_Name;

        private readonly bool m_IsActive;

        internal SwDm3rdPartyStorage(ISwDMDocument19 doc, string name, AccessType_e access) 
            : base(AccessTypeHelper.GetIsWriting(access))
        {
            m_Doc = doc;
            m_Name = name;
            m_IsActive = false;

            try
            {
                var storage = m_Doc.Get3rdPartyStorageStore(name, AccessTypeHelper.GetIsWriting(access)) as IComStorage;

                if (storage != null)
                {
                    Load(storage);
                    m_IsActive = true;
                }
                else 
                {
                    throw new Exception("Storage doesn't exist");
                }
            }
            catch 
            {
                m_Doc.Release3rdPartyStorageStore(m_Name);
                throw;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            if (m_IsActive)
            {
                if (!m_Doc.Release3rdPartyStorageStore(m_Name))
                {
                    throw new InvalidOperationException("Failed to release 3rd party storage store");
                }
            }
        }
    }
}
