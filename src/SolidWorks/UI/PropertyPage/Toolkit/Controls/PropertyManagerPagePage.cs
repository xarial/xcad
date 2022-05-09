//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using Xarial.XCad.Utils.PageBuilder.PageElements;

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

        private readonly ISwApplication m_App;

        internal PropertyManagerPagePage(IPropertyManagerPage2 page,
            SwPropertyManagerPageHandler handler, ISwApplication app, string helpLink, string whatsNewLink)
        {
            Page = page;
            Handler = handler;
            m_App = app;
            SwApp = app.Sw;
            m_HelpLink = helpLink;
            m_WhatsNewLink = whatsNewLink;

            Handler.HelpRequested += OnHelpRequested;
            Handler.WhatsNewRequested += OnWhatsNewRequested;
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
                Handler.HelpRequested -= OnHelpRequested;
                Handler.WhatsNewRequested -= OnWhatsNewRequested;
            }
        }
    }
}