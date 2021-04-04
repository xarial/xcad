using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Xarial.XCad.SolidWorks.Services
{
    public interface ITaskPaneControlProvider
    {
        object ProvideComControl(ITaskpaneView taskPaneView, string progId);
        bool ProvideNetControl(ITaskpaneView taskPaneView, Control ctrl);
    }

    internal class TaskPaneControlProvider : ITaskPaneControlProvider
    {
        public object ProvideComControl(ITaskpaneView taskPaneView, string progId)
            => taskPaneView.AddControl(progId, "");

        public bool ProvideNetControl(ITaskpaneView taskPaneView, Control ctrl)
            => taskPaneView.DisplayWindowFromHandle(ctrl.Handle.ToInt32());
    }
}
