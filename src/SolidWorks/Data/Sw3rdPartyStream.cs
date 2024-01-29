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
using Xarial.XCad.Data.Enums;
using Xarial.XCad.SolidWorks.Data.Helpers;
using Xarial.XCad.Toolkit.Data;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Data
{
    internal class Sw3rdPartyStream : ComStream
    {
        private readonly IModelDoc2 m_Model;
        private readonly string m_Name;
        private readonly bool m_IsActive;

        internal Sw3rdPartyStream(IModelDoc2 model, string name, AccessType_e access) 
            : base(AccessTypeHelper.GetIsWriting(access), false)
        {
            m_Model = model;
            m_Name = name;
            m_IsActive = false;

            try
            {
                var stream = model.IGet3rdPartyStorage(name, AccessTypeHelper.GetIsWriting(access)) as IStream;

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
                m_Model.IRelease3rdPartyStorage(m_Name);
                throw;
            }

            Seek(0, SeekOrigin.Begin);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (m_IsActive)
            {
                m_Model.IRelease3rdPartyStorage(m_Name);
            }
        }
    }
}
