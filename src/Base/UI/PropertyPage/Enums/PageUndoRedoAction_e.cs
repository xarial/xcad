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
    /// Undo or redo action of <see cref="Delegates.PageUndoDelegate"/>
    /// </summary>
    public enum PageUndoRedoAction_e
    {
        /// <summary>
        /// Undo action
        /// </summary>
        Undo,

        /// <summary>
        /// Redo action
        /// </summary>
        Redo
    }
}
