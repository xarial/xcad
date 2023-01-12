//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Attributes;

namespace Xarial.XCad.UI.Exceptions
{
    /// <summary>
    /// Indicates that not handler for dynamic controls
    /// </summary>
    public class DynamicControlHandlerMissingException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DynamicControlHandlerMissingException(PropertyInfo prp) 
            : base($"{prp.Name} property set as dynamic controls, but dynamic control creation handler is not set in")
        {
        }
    }
}
