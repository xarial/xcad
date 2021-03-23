using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents.Enums
{
    /// <summary>
    /// Enumeration used in <see cref="IXConfiguration.BomChildrenSolving"/>
    /// </summary>
    public enum BomChildrenSolving_e
    {
        /// <summary>
        /// Show children of this configuration
        /// </summary>
        Show,

        /// <summary>
        /// Hide children of this configuration in the BOM
        /// </summary>
        Hide,

        /// <summary>
        /// Promote children of this configuration (dissolve the assembly)
        /// </summary>
        Promote
    }
}
