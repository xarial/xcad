//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.UI.PropertyPage.Enums
{
    /// <summary>
    /// options for the proeprty page creation
    /// </summary>
    [Flags]
    public enum PageOptions_e
    {
        /// <summary>
        /// Abort all active commands when displaying this page
        /// </summary>
        AbortCommands = 1,

        /// <summary>
        /// Close the page with ESC key
        /// </summary>
        CanEscapeCancel = 2,

        /// <summary>
        /// Handle keystrokes of the page
        /// </summary>
        /// <remarks>Use <see cref="IXPropertyPage{TDataModel}.KeystrokeHook"/> to handle the event</remarks>
        HandleKeystrokes = 4,

        /// <summary>
        /// Allows selecting the chain entitites while page is active
        /// </summary>
        SupportsChainSelection = 8,

        /// <summary>
        /// Supports isolation of the components
        /// </summary>
        SupportsIsolate = 16
    }
}