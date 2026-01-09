//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <inheritdoc/>
    public class ControlTagAttribute : Attribute, IControlTagAttribute
    {   
        /// <inheritdoc/>
        public object Tag { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tag">Unique tag</param>
        public ControlTagAttribute(object tag)
        {
            Tag = tag;
        }
    }
}