//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SolidWorks.Documents.Exceptions
{
    public class ConfigurationSpecificCutListPropertiesWriteNotSupportedException : NotSupportedException, IUserException
    {
        public ConfigurationSpecificCutListPropertiesWriteNotSupportedException() 
            : base("Modifying configuration specific cut-list properties is not supported") 
        {
        }
    }
}
