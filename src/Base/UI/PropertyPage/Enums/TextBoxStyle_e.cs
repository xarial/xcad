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
    public enum TextBoxStyle_e
    {
        None = 0,
        NotifyOnlyWhenFocusLost = 1,
        ReadOnly = 2,
        NoBorder = 4,
        Multiline = 8
    }
}