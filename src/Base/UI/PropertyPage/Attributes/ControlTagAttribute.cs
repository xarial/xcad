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
    public class ControlTagAttribute : Attribute, IControlTagAttribute
    {
        /// <summary>
        /// Tag associated with the control
        /// </summary>
        public object Tag { get; private set; }

        public ControlTagAttribute(object tag)
        {
            Tag = tag;
        }
    }
}