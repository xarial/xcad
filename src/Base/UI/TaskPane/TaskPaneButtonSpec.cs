using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.Structures;
using Xarial.XCad.UI.TaskPane.Enums;

namespace Xarial.XCad.UI.TaskPane
{
    public class TaskPaneButtonSpec : ButtonSpec
    {
        public TaskPaneStandardIcons_e? StandardIcon { get; set; }
    }

    internal class TaskPaneEnumButtonSpec<TEnum> : TaskPaneButtonSpec
        where TEnum : Enum
    {
        public TEnum Value { get; set; }
    }
}
