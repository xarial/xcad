//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Additional options for option box control
    /// </summary>
    public class OptionBoxOptionsAttribute : Attribute, IAttribute
    {
        public OptionBoxStyle_e Style { get; private set; }

        /// <summary>
        /// Assigns additional options (such as style) for this option box control
        /// </summary>
        /// <param name="style"></param>
        public OptionBoxOptionsAttribute(
            OptionBoxStyle_e style = 0)
        {
            Style = style;
        }
    }
}