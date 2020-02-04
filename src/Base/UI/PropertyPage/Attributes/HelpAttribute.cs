//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Provides the additional help links for the page
    /// </summary>
    /// <remarks>Applied to the model class</remarks>
    public class HelpAttribute : Attribute, IAttribute
    {
        public string HelpLink { get; private set; }
        public string WhatsNewLink { get; private set; }

        /// <summary>
        /// Constructor for specifying links to help resources
        /// </summary>
        /// <param name="helpLink">Link to help documentation</param>
        /// <param name="whatsNewLink">Link to what's new page</param>
        public HelpAttribute(string helpLink, string whatsNewLink = "")
        {
            HelpLink = helpLink;
            WhatsNewLink = whatsNewLink;
        }
    }
}