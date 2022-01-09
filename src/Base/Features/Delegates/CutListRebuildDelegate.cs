//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;

namespace Xarial.XCad.Features.Delegates
{
    /// <summary>
    /// Delegate for <see cref="IXCutListItemRepository.CutListRebuild"/> event
    /// </summary>
    /// <param name="cutList">Cut-list being rebuilt</param>
    public delegate void CutListRebuildDelegate(IXCutListItemRepository cutList);
}
