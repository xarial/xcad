//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.Structures;

namespace Xarial.XCad.UI.TaskPane
{
    public class TaskPaneSpec : ButtonGroupSpec
    {
        public TaskPaneButtonSpec[] Buttons { get; set; }
    }
}
