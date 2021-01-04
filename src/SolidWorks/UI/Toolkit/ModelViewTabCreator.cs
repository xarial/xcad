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

namespace Xarial.XCad.SolidWorks.UI.Toolkit
{
    internal class ModelViewTabCreator<TControl> : CustomControlCreator<string, TControl>
    {
        private readonly IServiceProvider m_SvcProvider;
        private readonly ModelViewManager m_ModelViewMgr;

        internal ModelViewTabCreator(ModelViewManager modelViewMgr, IServiceProvider svcProvider)
        {
            m_ModelViewMgr = modelViewMgr;
            m_SvcProvider = svcProvider;
        }

        protected override string HostComControl(string progId, string title, IXImage image,
            out TControl specCtrl)
        {
            using (var iconsConv = m_SvcProvider.GetService<IIconsCreator>())
            {
                var imgPath = iconsConv.ConvertIcon(new FeatMgrViewIcon(image)).First();
                specCtrl = (TControl)m_ModelViewMgr.AddControl3(title, progId, "", true);

                if (specCtrl != null)
                {
                    return title;
                }
                else 
                {
                    throw new ComControlHostException(progId);
                }
            }
        }

        protected override string HostNetControl(Control winCtrlHost,
            string title, IXImage image)
        {
            using (var iconsConv = m_SvcProvider.GetService<IIconsCreator>())
            {
                var imgPath = iconsConv.ConvertIcon(new FeatMgrViewIcon(image)).First();

                if (m_ModelViewMgr.DisplayWindowFromHandlex64(title, winCtrlHost.Handle.ToInt64(), true))
                {
                    return title;
                }
                else
                {
                    throw new NetControlHostException(winCtrlHost.Handle);
                }
            }
        }
    }
}
