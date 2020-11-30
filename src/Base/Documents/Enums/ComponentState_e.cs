using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents.Enums
{
    /// <summary>
    /// States of the component <see cref="IXComponent.State"/>
    /// </summary>
    [Flags]
    public enum ComponentState_e
    {
        /// <summary>
        /// Default state of the component
        /// </summary>
        Default = 0,

        /// <summary>
        /// Component is suppressed
        /// </summary>
        Suppressed = 1,

        /// <summary>
        /// Component is loaded in the rapid mode
        /// </summary>
        Rapid = 2,

        /// <summary>
        /// Component is loaded in view-only mode
        /// </summary>
        ViewOnly = 4
    }
}
