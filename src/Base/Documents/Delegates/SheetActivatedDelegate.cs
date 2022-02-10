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
    /// Delegate for <see cref="IXSheetRepository.SheetActivated"/> event
    /// </summary>
    /// <param name="drw">Drawing where the sheet is activating</param>
    /// <param name="newSheet">Activated sheet</param>
    public delegate void SheetActivatedDelegate(IXDrawing drw, IXSheet newSheet);
}