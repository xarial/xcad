//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Documents.Services;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Additional methods for the <see cref="IXDocument"/>
    /// </summary>
    public static class XDocumentExtension
    {
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
