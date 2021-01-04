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
    internal class FeatureManagerTabCreator<TControl> : CustomControlCreator<IFeatMgrView, TControl>
    {
        private readonly IServiceProvider m_SvcProvider;
        private readonly ModelViewManager m_ModelViewMgr;

        internal FeatureManagerTabCreator(ModelViewManager modelViewMgr, IServiceProvider svcProvider)
        {
            m_ModelViewMgr = modelViewMgr;
            m_SvcProvider = svcProvider;
        }

        protected override IFeatMgrView HostComControl(string progId, string title, IXImage image,
            out TControl specCtrl)
        {
            using (var iconsConv = m_SvcProvider.GetService<IIconsCreator>())
            {
                var imgPath = iconsConv.ConvertIcon(new FeatMgrViewIcon(image)).First();

                var featMgrView = m_ModelViewMgr.CreateFeatureMgrControl3(imgPath, progId, "", title,
                    (int)swFeatMgrPane_e.swFeatMgrPaneBottom) as IFeatMgrView;

                specCtrl = default;

                if (featMgrView != null)
                {
                    specCtrl = (TControl)featMgrView.GetControl();
                }

                if (specCtrl == null)
                {
                    throw new ComControlHostException(progId);
                }

                return featMgrView;
            }
        }

        protected override IFeatMgrView HostNetControl(Control winCtrlHost,
            string title, IXImage image)
        {
            using (var iconsConv = m_SvcProvider.GetService<IIconsCreator>())
            {
                var imgPath = iconsConv.ConvertIcon(new FeatMgrViewIcon(image)).First();

                var featMgrView = m_ModelViewMgr.CreateFeatureMgrWindowFromHandlex64(
                    imgPath, winCtrlHost.Handle.ToInt64(),
                    title,
                    (int)swFeatMgrPane_e.swFeatMgrPaneBottom) as IFeatMgrView;

                if (featMgrView != null)
                {
                    return featMgrView;
                }
                else
                {
                    throw new NetControlHostException(winCtrlHost.Handle);
                }
            }
        }
    }
}
