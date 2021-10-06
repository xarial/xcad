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
    /// Delegate for <see cref="IXPart.CutListRebuild"/> event
    /// </summary>
    /// <param name="part">Part where cut-list is rebuilt</param>
    public delegate void CutListRebuildDelegate(IXPart part);
}
