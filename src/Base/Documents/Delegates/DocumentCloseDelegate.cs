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
using Xarial.XCad.Documents.Structures;

namespace Xarial.XCad.Documents.Delegates
{
    /// <summary>
    /// Type of the closing document event used in <see cref="DocumentCloseDelegate"/>
    /// </summary>
    public enum DocumentCloseType_e 
    {
        /// <summary>
        /// Document is closed and unloaded from the memory
        /// </summary>
        Destroy,

        /// <summary>
        /// Document is closing but remains in the memory (e.g. in drawing or assembly)
        /// </summary>
        Hide
    }

    /// <summary>
    /// Delegate for <see cref="IXDocument.Closing"/> notification
    /// </summary>
    /// <param name="doc">Document being closed</param>
    /// <param name="type">Closing type</param>
    public delegate void DocumentCloseDelegate(IXDocument doc, DocumentCloseType_e type);
}
