//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SolidWorks.Documents.Exceptions
{
    public class ConfigurationSpecificCutListNotSupportedException : NotSupportedException, IUserException
    {
        public ConfigurationSpecificCutListNotSupportedException() 
            : base("Configuration specific cut-lists are not supported. Instead access cut-lists from an active configuration") 
        {
        }
    }
}
