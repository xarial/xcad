//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal class PropertyManagerPagePage : Page
    {
        internal IPropertyManagerPage2 Page { get; private set; }
        internal SwPropertyManagerPageHandler Handler { get; private set; }
        internal ISldWorks App { get; private set; }

        private string m_HelpLink;
        private string m_WhatsNewLink;

        internal PropertyManagerPagePage(IPropertyManagerPage2 page,
            SwPropertyManagerPageHandler handler, ISldWorks app, string helpLink, string whatsNewLink)
        {
            Page = page;
            Handler = handler;
            App = app;
            m_HelpLink = helpLink;
            m_WhatsNewLink = whatsNewLink;

            Handler.HelpRequested += OnHelpRequested;
            Handler.WhatsNewRequested += OnWhatsNewRequested;
        }

        private void OnWhatsNewRequested()
        {
            OpenLink(m_WhatsNewLink);
        }

        private void OnHelpRequested()
        {
            OpenLink(m_HelpLink);
        }

        private void OpenLink(string link)
        {
            if (!string.IsNullOrEmpty(link))
            {
                try
                {
                    System.Diagnostics.Process.Start(link);
                }
                catch
                {
                }
            }
            else
            {
                App.SendMsgToUser2("Not available",
                    (int)swMessageBoxIcon_e.swMbWarning, (int)swMessageBoxBtn_e.swMbOk);
            }
        }

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