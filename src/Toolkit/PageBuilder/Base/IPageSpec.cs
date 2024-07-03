//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.Utils.PageBuilder.Base
{
    /// <summary>
    /// Specification of the page
    /// </summary>
    public interface IPageSpec
    {
        /// <summary>
        /// Page title
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Page options
        /// </summary>
        PageOptions_e Options { get; }

        /// <summary>
        /// Page buttons
        /// </summary>
        PageButtons_e Buttons { get; }

        /// <summary>
        /// Lock page strategy
        /// </summary>
        LockPageStrategy_e LockPageStrategy { get; }
    }
}