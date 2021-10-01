//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Services;

namespace Xarial.XCad.Documents.Attributes
{
    /// <summary>
    /// This attribute can be used on the <see cref="IDocumentHandler"/> implementation to specify the scope where this handler should be created
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DocumentHandlerScopeAttribute : Attribute
    {
        /// <summary>
        /// Handler scope
        /// </summary>
        public DocumentHandlerScope_e Scope { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="scope">Scope of this document handler</param>
        public DocumentHandlerScopeAttribute(DocumentHandlerScope_e scope) 
        {
            Scope = scope;
        }
    }
}
