//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.UI.PropertyPage.Enums
{
    [Flags]
    public enum NumberBoxStyle_e
    {
        None = 0,
        ComboEditBox = 1,
        EditBoxReadOnly = 2,
        AvoidSelectionText = 4,
        NoScrollArrows = 8,
        Slider = 16,
        Thumbwheel = 32,
        SuppressNotifyWhileTracking = 64
    }
}