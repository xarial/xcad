//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.Toolkit.Data;

namespace Xarial.XCad.SolidWorks.Data
{
    internal class Sw3rdPartyStorage : ComStorage
    {
        private readonly IModelDoc2 m_Model;
        private readonly string m_Name;

        internal bool Exists { get; }

        internal Sw3rdPartyStorage(IModelDoc2 model, string name, bool write) 
            : base(write)
        {
            m_Model = model;
            m_Name = name;
            Exists = false;

            try
            {
                var storage = (IComStorage)model.Extension.IGet3rdPartyStorageStore(name, write);

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
            => m_Model.Extension.IRelease3rdPartyStorageStore(m_Name);

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
