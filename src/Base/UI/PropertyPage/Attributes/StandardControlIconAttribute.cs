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
    /// Provides additional attribution options (i.e. icons, labels, tooltips etc.) for all controls
    /// </summary>
    /// <remarks>Can be applied to any property which is bound to the property manager page control</remarks>
    public class StandardControlIconAttribute : Attribute, IAttribute
    {
        public BitmapLabelType_e Label { get; private set; } = 0;

        /// <summary>Constructor allowing specify the standard icon</summary>
        /// <param name="label">Control label</param>
        public StandardControlIconAttribute(BitmapLabelType_e label)
        {
            Label = label;
        }
    }
}