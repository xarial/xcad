//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;

namespace Xarial.XCad.Services
{
    /// <summary>
    /// Handler of the link of <see cref="ITooltipSpec.Link"/>
    /// </summary>
    public interface ITooltipLinkLinkHandler
    {
        /// <summary>
        /// Handles opening of the link
        /// </summary>
        /// <param name="tooltip">Caller tooltip</param>
        void OpenLink(ITooltipSpec tooltip);
    }
}
