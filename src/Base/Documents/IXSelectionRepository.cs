//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Graphics;
using Xarial.XCad.UI;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Handles the selection objects
    /// </summary>
    public interface IXSelectionRepository : IXRepository<IXSelObject>
    {
        /// <summary>
        /// Raised when new object is selected
        /// </summary>
        event NewSelectionDelegate NewSelection;

        /// <summary>
        /// Raised when the selection is cleared
        /// </summary>
        event ClearSelectionDelegate ClearSelection;

        /// <summary>
        /// Clears all current selections
        /// </summary>
        void Clear();

        /// <summary>
        /// Pre-creates selection callout instance
        /// </summary>
        /// <returns>Instance of the selection callout</returns>
        IXSelCallout PreCreateCallout();
    }
}
