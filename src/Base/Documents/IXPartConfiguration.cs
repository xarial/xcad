//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Features;
using System.Collections.Generic;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the configuration of <see cref="IXPart"/>
    /// </summary>
    public interface IXPartConfiguration : IXConfiguration
    {
        /// <summary>
        /// Cut-list items in this configuration (if available)
        /// </summary>
        IXCutListItemRepository CutLists { get; }

        /// <summary>
        /// Material of this part
        /// </summary>
        IXMaterial Material { get; set; }
    }
}