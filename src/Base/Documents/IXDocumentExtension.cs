//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Enums;

namespace Xarial.XCad.Documents
{
    public static class IXDocumentExtension
    {
        public static Stream TryOpenStream(this IXDocument doc, string name, AccessType_e access) 
        {
            try
            {
                return doc.OpenStream(name, access);
            }
            catch 
            {
                return null;
            }
        }

        public static IStorage TryOpenStorage(this IXDocument doc, string name, AccessType_e access)
        {
            try
            {
                return doc.OpenStorage(name, access);
            }
            catch
            {
                return null;
            }
        }
    }
}
