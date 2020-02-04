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
    public enum SelectionBoxStyle_e
    {
        None = 0,
        HorizontalScroll = 1,
        UpAndDownButtons = 2,
        MultipleItemSelect = 4,
        WantListboxSelectionChanged = 8
    }
}