using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Features;

namespace Xarial.XCad.Enums
{
    /// <summary>
    /// Represents the <see cref="IXCutListItem.State"/> of the cut-list
    /// </summary>
    [Flags]
    public enum CutListState_e
    {
        /// <summary>
        /// Cut-list is excluded from BOM
        /// </summary>
        ExcludeFromBom = 1
    }
}
