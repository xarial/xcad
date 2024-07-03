//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.UI.PropertyPage.Enums
{
    /// <summary>
    /// Buttons of the <see cref="IXPropertyPage{TDataModel}"/>
    /// </summary>
    [Flags]
    public enum PageButtons_e
    {
        /// <summary>
        /// OK button
        /// </summary>
        Okay = 1,

        /// <summary>
        /// Cancel button
        /// </summary>
        Cancel = 2,

        /// <summary>
        /// Pushpin button
        /// </summary>
        Pushpin = 4,

        /// <summary>
        /// Preview button
        /// </summary>
        /// <remarks>Handle <see cref="IXPropertyPage{TDataModel}.Preview"/> event to update the preview</remarks>
        Preview = 8,

        /// <summary>
        /// Undo button
        /// </summary>
        Undo = 16,

        /// <summary>
        /// Redu button
        /// </summary>
        Redo = 32
    }
}
