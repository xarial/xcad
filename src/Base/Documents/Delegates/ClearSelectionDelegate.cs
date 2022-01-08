//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents.Delegates
{
    /// <summary>
    /// Delegate for <see cref="IXSelectionRepository.ClearSelection"/> event
    /// </summary>
    /// <param name="doc">Document where the selection is cleared</param>
    public delegate void ClearSelectionDelegate(IXDocument doc);
}
