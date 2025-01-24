//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.Documents.Extensions
{
    /// <summary>
    /// Additional methods for <see cref="IXDocument"/>
    /// </summary>
    public static class XDocumentExtension
    {
        /// <summary>
        /// Saves the document as new file
        /// </summary>
        /// <param name="doc">Input document</param>
        /// <param name="filePath">Output file path</param>
        public static void SaveAs(this IXDocument doc, string filePath) 
        {
            var oper = doc.PreCreateSaveAsOperation(filePath);
            oper.Commit();
        }

        /// <summary>
        /// Saves the document and configure options
        /// </summary>
        /// <typeparam name="T">Type of save operation</typeparam>
        /// <param name="doc">Input document</param>
        /// <param name="filePath">Output file path</param>
        /// <param name="configurer">Specific configurer</param>
        public static void SaveAs<T>(this IXDocument doc, string filePath, Action<T> configurer)
            where T : IXSaveOperation
        {
            var oper = (T)doc.PreCreateSaveAsOperation(filePath);
            configurer.Invoke(oper);
            oper.Commit();
        }
    }
}
