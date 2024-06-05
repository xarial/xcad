﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
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
        /// <summary>
        /// Assigns additional options (such as style) for this option box control
        /// </summary>
        public OptionBoxOptionsAttribute()
        {
        }
    }
}