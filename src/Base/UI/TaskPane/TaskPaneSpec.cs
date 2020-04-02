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
