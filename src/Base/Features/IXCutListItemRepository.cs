//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Features;
using Xarial.XCad.Features.Delegates;

namespace Xarial.XCad.Features
{
    /// <summary>
    /// Represents the collection of cut-list items
    /// </summary>
    public interface IXCutListItemRepository : IXRepository<IXCutListItem>
    {
        /// <summary>
        /// Fired when cut list is regenerated
        /// </summary>
        event CutListRebuildDelegate CutListRebuild;
    }
}
