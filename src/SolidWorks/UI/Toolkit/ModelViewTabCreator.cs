//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI;
using Xarial.XCad.Toolkit;
using System.Linq;
using System.Windows.Forms;
using Xarial.XCad.SolidWorks.UI.Commands.Exceptions;
using Xarial.XCad.Toolkit.Windows.UI;

namespace Xarial.XCad.SolidWorks.UI.Toolkit
{
    internal class ModelViewTabCreator<TControl> : CustomControlHost<string, TControl>
    {
        private readonly IServiceProvider m_SvcProvider;
        private readonly ModelViewManager m_ModelViewMgr;
        private readonly IModelViewControlProvider m_CtrlProvider;

        internal ModelViewTabCreator(ModelViewManager modelViewMgr, IServiceProvider svcProvider)
        {
            m_ModelViewMgr = modelViewMgr;
            m_SvcProvider = svcProvider;
            m_CtrlProvider = m_SvcProvider.GetService<IModelViewControlProvider>();
        }

        protected override string HostComControl(string progId, string title, IXImage image,
            out TControl specCtrl)
        {
            specCtrl = (TControl)m_CtrlProvider.ProvideComControl(m_ModelViewMgr, progId, title);

            if (specCtrl != null)
            {
                return title;
            }
            else
            {
                throw new ComControlHostException(progId);
            }
        }

        protected override string HostWinFormsControl(Control winCtrl, string title, IXImage image)
        {
            if (m_CtrlProvider.ProvideNetControl(m_ModelViewMgr, winCtrl, title))
            {
                return title;
            }
            else
            {
                throw new NetControlHostException(winCtrl.Handle);
            }
        }
    }
}
