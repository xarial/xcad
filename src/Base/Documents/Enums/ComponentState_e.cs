//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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
        Lightweight = 2,

        /// <summary>
        /// Component is loaded in view-only mode
        /// </summary>
        ViewOnly = 4,

        /// <summary>
        /// Component is hidden
        /// </summary>
        Hidden = 8,

        /// <summary>
        /// Component is excluded from Bill Of Materials
        /// </summary>
        ExcludedFromBom = 16,

        /// <summary>
        /// Components is created as envelope
        /// </summary>
        Envelope = 32
    }
}
