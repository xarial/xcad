﻿//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Property manager page options
    /// </summary>
    /// <remarks>Applied to the main class of the data model</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PageOptionsAttribute : Attribute, IAttribute
    {
        /// <summary>
        /// Options of the page
        /// </summary>
        public PageOptions_e Options { get; private set; }

        /// <summary>Constructor for specifying property manager page options</summary>
        /// <param name="opts">property page options</param>
        public PageOptionsAttribute(PageOptions_e opts)
        {
            Options = opts;
        }
    }
}