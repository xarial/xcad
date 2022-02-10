//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Windows.Forms;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.Commands.Exceptions;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI;
using Xarial.XCad.Toolkit;
using System.Linq;

namespace Xarial.XCad.SolidWorks.UI.Toolkit
{
    internal class FeatureManagerTabCreator<TControl> : CustomControlCreator<Tuple<IFeatMgrView, string>, TControl>
    {
        private readonly IServiceProvider m_SvcProvider;
        private readonly ModelViewManager m_ModelViewMgr;
        private readonly IFeatureManagerTabControlProvider m_TabProvider;

        internal FeatureManagerTabCreator(ModelViewManager modelViewMgr, IServiceProvider svcProvider)
        {
            m_ModelViewMgr = modelViewMgr;
            m_SvcProvider = svcProvider;
            m_TabProvider = m_SvcProvider.GetService<IFeatureManagerTabControlProvider>();
        }

        protected override Tuple<IFeatMgrView, string> HostComControl(string progId, string title, IXImage image,
            out TControl specCtrl)
        {
            using (var iconsConv = m_SvcProvider.GetService<IIconsCreator>())
            {
                var imgPath = iconsConv.ConvertIcon(new FeatMgrViewIcon(image)).First();

                var featMgrView = m_TabProvider.ProvideComControl(m_ModelViewMgr, imgPath, progId, title);

                specCtrl = default;

                if (featMgrView != null)
                {
                    specCtrl = (TControl)featMgrView.GetControl();
                }

                if (specCtrl == null)
                {
                    throw new ComControlHostException(progId);
                }

                return new Tuple<IFeatMgrView, string>(featMgrView, title);
            }
        }

        protected override Tuple<IFeatMgrView, string> HostNetControl(Control winCtrlHost, TControl ctrl,
            string title, IXImage image)
        {
            using (var iconsConv = m_SvcProvider.GetService<IIconsCreator>())
            {
                var imgPath = iconsConv.ConvertIcon(new FeatMgrViewIcon(image)).First();

                var featMgrView = m_TabProvider.ProvideNetControl(m_ModelViewMgr, winCtrlHost, imgPath, title);
                
                if (featMgrView != null)
                {
                    return new Tuple<IFeatMgrView, string>(featMgrView, title);
                }
                else
                {
                    throw new NetControlHostException(winCtrlHost.Handle);
                }
            }
        }
    }
}
