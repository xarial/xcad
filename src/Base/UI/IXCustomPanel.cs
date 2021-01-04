//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.UI
{
    /// <summary>
    /// Represents the panel with custom User Control
    /// </summary>
    /// <typeparam name="TControl">Type of user control</typeparam>
    public interface IXCustomPanel<TControl>
    {
        /// <summary>
        /// Checks if this panel is active
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// Returns the specific User Control of this panel
        /// </summary>
        TControl Control { get; }
        
        /// <summary>
        /// Closes current panel
        /// </summary>
        void Close();
    }
}
