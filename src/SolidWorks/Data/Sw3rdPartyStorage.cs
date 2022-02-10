//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Data.Enums;
using Xarial.XCad.SolidWorks.Data.Helpers;
using Xarial.XCad.Toolkit.Data;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Data
{
    internal class Sw3rdPartyStorage : ComStorage
    {
        private readonly IModelDoc2 m_Model;
        private readonly string m_Name;

        private readonly bool m_IsActive;

        internal Sw3rdPartyStorage(IModelDoc2 model, string name, AccessType_e access) 
            : base(AccessTypeHelper.GetIsWriting(access))
        {
            m_Model = model;
            m_Name = name;
            m_IsActive = false;

            try
            {
                var storage = model.Extension.IGet3rdPartyStorageStore(name, AccessTypeHelper.GetIsWriting(access)) as IComStorage;

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
                m_Model.Extension.IRelease3rdPartyStorageStore(m_Name);
                throw;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            if (m_IsActive)
            {
                if (!m_Model.Extension.IRelease3rdPartyStorageStore(m_Name))
                {
                    throw new InvalidOperationException("Failed to release 3rd party storage store");
                }
            }
        }
    }
}
