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
    public enum ComboBoxStyle_e
    {
        Sorted = 1,
        EditableText = 2,
        EditBoxReadOnly = 4,
        AvoidSelectionText = 8
    }
}