//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.UI.PropertyPage.Structures
{
    /// <summary>
    /// Argument of the <see cref="Delegates.PageNavigationDelegate"/>
    /// </summary>
    public class PageNavigationArg
    {
        /// <summary>
        /// True to cancel the page navigation
        /// </summary>
        public bool Cancel { get; set; }
    }
}
