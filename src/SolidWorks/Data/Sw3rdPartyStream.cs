//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Xarial.XCad.Toolkit.Data;

namespace Xarial.XCad.SolidWorks.Data
{
    internal class Sw3rdPartyStream : ComStream
    {
        private readonly IModelDoc2 m_Model;
        private readonly string m_Name;
        
        internal bool Exists { get; }

        internal Sw3rdPartyStream(IModelDoc2 model, string name, bool write) 
            : base(write, false)
        {
            m_Model = model;
            m_Name = name;
            Exists = false;

            try
            {
                var stream = (IStream)model.IGet3rdPartyStorage(name, write);

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

        private void Release()
            => m_Model.IRelease3rdPartyStorage(m_Name);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (Exists)
            {
                Release();
            }
        }
    }
}
