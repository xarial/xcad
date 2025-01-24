//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Features;

namespace Xarial.XCad.Enums
{
    /// <summary>
    /// Represents the <see cref="IXCutListItem.Status"/> of the cut-list
    /// </summary>
    [Flags]
    public enum CutListStatus_e
    {
        /// <summary>
        /// Cut-list is excluded from BOM
        /// </summary>
        ExcludeFromBom = 1
    }
}
