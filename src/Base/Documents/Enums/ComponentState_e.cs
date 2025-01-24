//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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
        /// Component is loaded in the lightweight mode
        /// </summary>
        Lightweight = 2 << 0,

        /// <summary>
        /// Component is loaded in view-only mode
        /// </summary>
        ViewOnly = 2 << 1,

        /// <summary>
        /// Component is hidden
        /// </summary>
        Hidden = 2 << 2,

        /// <summary>
        /// Component is excluded from Bill Of Materials
        /// </summary>
        ExcludedFromBom = 2 << 3,

        /// <summary>
        /// Components is created as envelope
        /// </summary>
        Envelope = 2 << 4,

        /// <summary>
        /// Component is embedded (virtual) into the assembly
        /// </summary>
        Embedded = 2 << 5,

        /// <summary>
        /// Indicates that component is suppressed due to the mismatched ID of its underlying model
        /// </summary>
        SuppressedIdMismatch = 2 << 6,

        /// <summary>
        /// Component has a fixed position
        /// </summary>
        Fixed = 2 << 7,

        /// <summary>
        /// Component is a foreign document (interconnected)
        /// </summary>
        Foreign = 2 << 8
    }
}
