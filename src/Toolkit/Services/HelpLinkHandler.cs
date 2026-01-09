//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.UI.PropertyPage.Services;

namespace Xarial.XCad.Toolkit.Services
{
    /// <summary>
    /// Default help link handler which executes the link
    /// </summary>
    public class HelpLinkHandler : IHelpLinkHandler
    {

        private readonly IXApplication m_App;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="app">Pointer to application</param>
        public HelpLinkHandler(IXApplication app)
        {
            m_App = app;
        }

        /// <inheritdoc/>
        public void OpenHelpLink(string link) => TryOpenLink(link);

        /// <inheritdoc/>
        public void OpenWhatsNewLink(string whatsNewLink) => TryOpenLink(whatsNewLink);

        private void TryOpenLink(string link) => LinkHelper.TryOpenLink(link, m_App, nameof(IHelpLinkHandler));
    }
}
