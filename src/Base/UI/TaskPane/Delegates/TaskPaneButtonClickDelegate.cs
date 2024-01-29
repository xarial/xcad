//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.Structures;
using Xarial.XCad.UI.TaskPane;

namespace Xarial.XCad.UI.TaskPane.Delegates
{
    public delegate void TaskPaneButtonClickDelegate(TaskPaneButtonSpec spec);

    public delegate void TaskPaneButtonEnumClickDelegate<TCmdEnum>(TCmdEnum spec)
        where TCmdEnum : Enum;
}