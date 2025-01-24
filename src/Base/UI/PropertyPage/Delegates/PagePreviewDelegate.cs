//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.UI.PropertyPage.Delegates
{
    /// <summary>
    /// Delegate of <see cref="IXPropertyPage{TDataModel}.Preview"/> event
    /// </summary>
    /// <param name="enabled">True if preview is enabled, false if disabled</param>
    /// <param name="cancel">Cancel the preview state change</param>
    public delegate void PagePreviewDelegate(bool enabled, ref bool cancel);
}
