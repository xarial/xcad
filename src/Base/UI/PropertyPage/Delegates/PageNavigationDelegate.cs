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
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Structures;

namespace Xarial.XCad.UI.PropertyPage.Delegates
{
    /// <summary>
    /// Delegate of <see cref="IXPropertyPage{TDataModel}.Navigate"/>
    /// </summary>
    /// <param name="action">Undo action type</param>
    /// <param name="arg">Navigation argument</param>
    public delegate void PageNavigationDelegate(PageNavigationAction_e action, PageNavigationArg arg);
}
