//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal class PropertyManagerPagePage : Page
    {
        internal IPropertyManagerPage2 Page { get; }
        internal SwPropertyManagerPageHandler Handler { get; }
        internal ISldWorks SwApp { get; }
        
        public override bool Enabled 
        {
            get => throw new NotSupportedException(); 
            set => throw new NotSupportedException(); 
        }

        public override bool Visible
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        private string m_HelpLink;
        private string m_WhatsNewLink;

        private readonly SwApplication m_App;

        private IImageCollection m_PageIcon;

        internal PropertyManagerPagePage(SwApplication app, IAttributeSet atts, IIconsCreator iconsConv, SwPropertyManagerPageHandler handler) 
        {
            m_App = app;

            Handler = handler;
            
            int err = -1;

            swPropertyManagerPageOptions_e opts;

            TitleIcon titleIcon = null;

            IconAttribute commIconAtt;
            if (atts.ContextType.TryGetAttribute(out commIconAtt))
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

                m_HelpLink = helpAtt.HelpLink;
                m_WhatsNewLink = helpAtt.WhatsNewLink;

                Handler.HelpRequested += OnHelpRequested;
                Handler.WhatsNewRequested += OnWhatsNewRequested;
            }

            Page = m_App.Sw.CreatePropertyManagerPage(atts.Name,
                (int)opts,
                Handler, ref err) as IPropertyManagerPage2;

            if (titleIcon != null)
            {
                m_PageIcon = iconsConv.ConvertIcon(titleIcon);
                Page.SetTitleBitmap2(m_PageIcon.FilePaths[0]);
            }

            if (atts.Has<MessageAttribute>())
            {
                var msgAtt = atts.Get<MessageAttribute>();
                Page.SetMessage3(msgAtt.Text, (int)msgAtt.Visibility,
                    (int)msgAtt.Expanded, msgAtt.Caption);
            }
            else if (!string.IsNullOrEmpty(atts.Description))
            {
                Page.SetMessage3(atts.Description, (int)swPropertyManagerPageMessageVisibility.swMessageBoxVisible,
                    (int)swPropertyManagerPageMessageExpanded.swMessageBoxExpand, "");
            }
        }

        internal void Show()
        {
            const int OPTS_DEFAULT = 0;

            Page.Show2(OPTS_DEFAULT);

            //NOTE: help button cannot be hidden, only disabled
            if (string.IsNullOrEmpty(m_HelpLink))
            {
                Page.EnableButton((int)swPropertyManagerPageButtons_e.swPropertyManagerPageButton_Help, false);
            }
        }

        public override void ShowTooltip(string title, string msg)
        {
            SwApp.HideBubbleTooltip();
            SwApp.ShowBubbleTooltipAt2(0, 0, (int)swArrowPosition.swArrowLeftTop,
                        title, msg, (int)swBitMaps.swBitMapNone,
                        "", "", 0, (int)swLinkString.swLinkStringNone, "", "");
        }

        private void OnWhatsNewRequested()
            => this.TryOpenLink(m_WhatsNewLink, m_App);

        private void OnHelpRequested()
            => this.TryOpenLink(m_HelpLink, m_App);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                base.Dispose(disposing);

                Handler.HelpRequested -= OnHelpRequested;
                Handler.WhatsNewRequested -= OnWhatsNewRequested;

                m_PageIcon?.Dispose();
            }
        }
    }
}