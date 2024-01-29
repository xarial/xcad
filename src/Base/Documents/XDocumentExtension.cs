//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Enums;
using Xarial.XCad.Documents.Services;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Additional methods for the <see cref="IXDocument"/>
    /// </summary>
    public static class XDocumentExtension
    {
        /// <summary>
        /// Tries to open stream from the document
        /// </summary>
        /// <param name="doc">Document to open stream from</param>
        /// <param name="name">Name of the stream</param>
        /// <param name="access">Type of the access</param>
        /// <returns>Stream or null</returns>
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

        /// <summary>
        /// Tries to open storage from the document
        /// </summary>
        /// <param name="doc">Document to open storage from</param>
        /// <param name="name">Name of the storage</param>
        /// <param name="access">Type of the access</param>
        /// <returns>Storage or null</returns>
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

        /// <summary>
        /// Creates operations group
        /// </summary>
        /// <param name="doc">Document</param>
        /// <param name="name">Name of the operation</param>
        /// <param name="isTemp">True if operation is temp</param>
        /// <returns>Operation group</returns>
        public static IOperationGroup CreateOperationGroup(this IXDocument doc, string name, bool isTemp) 
        {
            var operGrp = doc.PreCreateOperationGroup();
            operGrp.Name = name;
            operGrp.IsTemp = isTemp;

            operGrp.Commit();

            return operGrp;
        }
    }
}
