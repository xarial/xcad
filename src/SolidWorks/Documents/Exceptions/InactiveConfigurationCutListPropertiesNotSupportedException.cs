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
    public class InactiveConfigurationCutListPropertiesNotSupportedException : Exception, IUserException
    {
        public InactiveConfigurationCutListPropertiesNotSupportedException() 
            : base("Due to current limitations cut-lists are only supported in the active configuration") 
        {
        }
    }
}
