//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.UI.PropertyPage.Delegates
{
    /// <summary>
    /// Delegate of <see cref="IXPropertyPage{TDataModel}.Closed"/> event
    /// </summary>
    /// <param name="reason">Reason of page closing</param>
    public delegate void PageClosedDelegate(PageCloseReasons_e reason);
}