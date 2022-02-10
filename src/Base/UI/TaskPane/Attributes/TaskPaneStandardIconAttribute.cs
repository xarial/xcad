//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.TaskPane.Enums;

namespace Xarial.XCad.UI.TaskPane.Attributes
{
    public class TaskPaneStandardIconAttribute : Attribute
    {
        public TaskPaneStandardIcons_e StandardIcon { get; }

        public TaskPaneStandardIconAttribute(TaskPaneStandardIcons_e standardIcon) 
        {
            StandardIcon = standardIcon;
        }
    }
}
