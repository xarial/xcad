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
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Customizing proeprty page buttons
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PageButtonsAttribute : Attribute, IAttribute
    {
        /// <summary>
        /// List of buttons
        /// </summary>
        public PageButtons_e Buttons { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buttons">Buttons list</param>
        public PageButtonsAttribute(PageButtons_e buttons) 
        {
            Buttons = buttons;
        }
    }
}
