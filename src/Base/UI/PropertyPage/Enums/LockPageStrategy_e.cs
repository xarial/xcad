//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.UI.PropertyPage.Enums
{
    /// <summary>
    /// Type of the locked page
    /// </summary>
    [Flags]
    public enum LockPageStrategy_e 
    {
        /// <summary>
        /// Inidcates that page must be displayed as modal
        /// </summary>
        /// <remarks>This may block som eof the commands performed via page is open</remarks>
        Blocked = 1,

        /// <summary>
        /// Indicates that page should be reopened if closed when other command is run
        /// </summary>
        Restorable = 2,

        /// <summary>
        /// Forbid selection while page is open
        /// </summary>
        DisableSelection = 4
    }
}
