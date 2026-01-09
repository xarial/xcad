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
using Xarial.XCad.Base;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Services;
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.UI.PropertyPage.Services;

namespace Xarial.XCad.Toolkit.Services
{
    /// <summary>
    /// Default tooltip link handler which executes the link
    /// </summary>
    public class TooltipLinkLinkHandler : ITooltipLinkLinkHandler
    {
        private readonly IXApplication m_App;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="app">Pointer to application</param>
        public TooltipLinkLinkHandler(IXApplication app)
        {
            m_App = app;
        }

        /// <inheritdoc/>
        public void OpenLink(ITooltipSpec tooltip) => LinkHelper.TryOpenLink(tooltip.Link, m_App, nameof(ITooltipLinkLinkHandler));
    }
}
