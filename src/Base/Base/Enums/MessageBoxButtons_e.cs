//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Base.Enums
{
    /// <summary>
    /// Specifies the buttons to display in <see cref="IXApplication.ShowMessageBox(string, MessageBoxIcon_e, MessageBoxButtons_e)"/>
    /// </summary>
    public enum MessageBoxButtons_e
    {
        /// <summary>
        /// OK button only
        /// </summary>
        Ok,

        /// <summary>
        /// OK and Cancel buttons
        /// </summary>
        OkCancel,

        /// <summary>
        /// Yes and No buttons
        /// </summary>
        YesNo,

        /// <summary>
        /// Yes, No and Cancel buttons
        /// </summary>
        YesNoCancel
    }
}
