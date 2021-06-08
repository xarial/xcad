//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Features.CustomFeature.Attributes
{
    /// <summary>
    /// Use this attribute to provide a user friendly message when definition of custom feature is not registered
    /// </summary>
    /// <remarks>The message might point to the download page of your extension or list a contact e-mail</remarks>
    public class MissingDefinitionErrorMessage : Attribute
    {
        /// <summary>
        /// Missing custom feature definition error message
        /// </summary>
        public string Message { get; }

        ///<summary>Constructor to specify option</summary>
        /// <param name="msg">Default message to display when custom feature cannot be loaded
        /// The provided text is displayed in the What's Wrong dialog of</param>
        public MissingDefinitionErrorMessage(string msg)
        {
            Message = msg;
        }
    }
}
