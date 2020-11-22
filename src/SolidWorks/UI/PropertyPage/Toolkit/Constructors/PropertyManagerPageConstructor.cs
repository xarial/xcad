//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Linq;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Constructors;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    internal class PropertyManagerPageConstructor : PageConstructor<PropertyManagerPagePage>
    {
        private readonly ISldWorks m_App;
        private readonly IIconsCreator m_IconsConv;
        private readonly SwPropertyManagerPageHandler m_Handler;

        internal PropertyManagerPageConstructor(ISldWorks app, IIconsCreator iconsConv, SwPropertyManagerPageHandler handler)
        {
            m_App = app;
            m_IconsConv = iconsConv;

            m_Handler = handler;
            handler.Init(m_App);
        }

        protected override PropertyManagerPagePage Create(IAttributeSet atts)
        {
            int err = -1;

            swPropertyManagerPageOptions_e opts;

            TitleIcon titleIcon = null;

            IconAttribute commIconAtt;
            if (atts.BoundType.TryGetAttribute(out commIconAtt))
            {
                if (commIconAtt.Icon != null)
                {
                    titleIcon = new TitleIcon(commIconAtt.Icon);
                }
            }

            if (atts.Has<PageOptionsAttribute>())
            {
                var optsAtt = atts.Get<PageOptionsAttribute>();

                //TODO: implement conversion
                opts = (swPropertyManagerPageOptions_e)optsAtt.Options;
            }
            else
            {
                //TODO: implement conversion
                opts = (swPropertyManagerPageOptions_e)(PageOptions_e.OkayButton | PageOptions_e.CancelButton);
            }

            var helpLink = "";
            var whatsNewLink = "";

            if (atts.Has<HelpAttribute>())
            {
                var helpAtt = atts.Get<HelpAttribute>();

                if (!string.IsNullOrEmpty(helpAtt.WhatsNewLink))
                {
                    if (!opts.HasFlag(swPropertyManagerPageOptions_e.swPropertyManagerOptions_WhatsNew))
                    {
                        opts |= swPropertyManagerPageOptions_e.swPropertyManagerOptions_WhatsNew;
                    }
                }

                helpLink = helpAtt.HelpLink;
                whatsNewLink = helpAtt.WhatsNewLink;
            }

            var page = m_App.CreatePropertyManagerPage(atts.Name,
                (int)opts,
                m_Handler, ref err) as IPropertyManagerPage2;

            if (titleIcon != null)
            {
                var iconPath = m_IconsConv.ConvertIcon(titleIcon).First();
                page.SetTitleBitmap2(iconPath);
            }

            if (atts.Has<MessageAttribute>())
            {
                var msgAtt = atts.Get<MessageAttribute>();
                page.SetMessage3(msgAtt.Text, (int)msgAtt.Visibility,
                    (int)msgAtt.Expanded, msgAtt.Caption);
            }
            else if (!string.IsNullOrEmpty(atts.Description))
            {
                page.SetMessage3(atts.Description, (int)swPropertyManagerPageMessageVisibility.swMessageBoxVisible,
                    (int)swPropertyManagerPageMessageExpanded.swMessageBoxExpand, "");
            }

            return new PropertyManagerPagePage(page, m_Handler, m_App, helpLink, whatsNewLink);
        }
    }
}