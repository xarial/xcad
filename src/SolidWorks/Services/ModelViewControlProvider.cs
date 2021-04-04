using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Xarial.XCad.SolidWorks.Services
{
    public interface IModelViewControlProvider
    {
        object ProvideComControl(IModelViewManager mdlViewMgr, string progId, string title);
        bool ProvideNetControl(IModelViewManager mdlViewMgr, Control ctrl, string title);
    }

    internal class ModelViewControlProvider : IModelViewControlProvider
    {
        public object ProvideComControl(IModelViewManager mdlViewMgr, string progId, string title)
            => mdlViewMgr.AddControl3(title, progId, "", true);

        public bool ProvideNetControl(IModelViewManager mdlViewMgr, Control ctrl, string title)
            => mdlViewMgr.DisplayWindowFromHandlex64(title, ctrl.Handle.ToInt64(), true);
    }
}
