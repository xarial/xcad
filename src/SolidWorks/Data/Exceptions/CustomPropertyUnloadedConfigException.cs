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

namespace Xarial.XCad.SolidWorks.Data.Exceptions
{
    public class CustomPropertyUnloadedConfigException : Exception, IUserException
    {
        public CustomPropertyUnloadedConfigException() 
            : base("Custom property is not added to unloaded configuration. Try activate configuration before adding the property") 
        {
        }
    }
}
