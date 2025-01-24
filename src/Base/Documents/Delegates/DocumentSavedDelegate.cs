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
    /// Delegate for <see cref="IXDocument.Saved"/> event
    /// </summary>
    /// <param name="doc">Document being saved</param>
    /// <param name="type">Save type</param>
    /// <param name="cancelled">True if save was cancelled</param>
    public delegate void DocumentSavedDelegate(IXDocument doc, DocumentSaveType_e type, bool cancelled);
}
