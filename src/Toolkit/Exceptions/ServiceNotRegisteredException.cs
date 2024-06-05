﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Toolkit.Exceptions
{
    public class ServiceNotRegisteredException : Exception
    {
        public ServiceNotRegisteredException(Type serviceType) : base($"Service '{serviceType.FullName}' is not registered")
        { 
        }
    }
}
