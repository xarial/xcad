//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
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
    public class DocumentHandlerFilterAttribute : Attribute
    {
        /// <summary>
        /// Handler scope
        /// </summary>
        public Type[] Filters { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="filters">Filters for document handler (e.g. <see cref="IXPart"/></param>, <see cref="IXAssembly"/>, <see cref="IXDrawing"/>)
        public DocumentHandlerFilterAttribute(params Type[] filters) 
        {
            Filters = filters;
        }
    }
}
