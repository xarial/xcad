//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
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
using Xarial.XCad.UI.PropertyPage.Attributes;

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

        /// <summary>
        /// Option to automatically generate cut-lists
        /// </summary>
        bool AutomaticCutList { get; set; }
        
        /// <summary>
        /// Option to automatically update cut-lists folder
        /// </summary>
        bool AutomaticUpdate { get; set; }
        
        /// <summary>
        /// Updates cut-lists folder
        /// </summary>
        /// <returns></returns>
        bool Update();
    }
}
