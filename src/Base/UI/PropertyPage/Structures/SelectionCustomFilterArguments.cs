//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.UI.PropertyPage.Structures
{
    /// <summary>
    /// Arguments of <see cref="Services.ISelectionCustomFilter"/>
    /// </summary>
    public class SelectionCustomFilterArguments
    {
        /// <summary>
        /// Text of the item to be displayed in the selection box
        /// </summary>
        public string ItemText { get; set; }

        /// <summary>
        /// True to allow this item to be selected
        /// </summary>
        public bool Filter { get; set; }

        /// <summary>
        /// Reason to display to the user of <see cref="Filter"/> is False
        /// </summary>
        public string Reason { get; set; }
    }
}
