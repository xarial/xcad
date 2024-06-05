//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
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
    /// Delegate for <see cref="IXDocument.Saving"/> event
    /// </summary>
    /// <param name="doc">Document being saved</param>
    /// <param name="type">Save type</param>
    /// <param name="args">Savig arguments</param>
    public delegate void DocumentSaveDelegate(IXDocument doc, DocumentSaveType_e type, DocumentSaveArgs args);
}
