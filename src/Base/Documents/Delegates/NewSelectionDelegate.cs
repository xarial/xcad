//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents.Delegates
{
    /// <summary>
    /// Delegate for the <see cref="IXSelectionRepository.NewSelection"/> event
    /// </summary>
    /// <param name="doc">Document where selection is done</param>
    /// <param name="selObject">Selected object</param>
    public delegate void NewSelectionDelegate(IXDocument doc, IXSelObject selObject);
}
