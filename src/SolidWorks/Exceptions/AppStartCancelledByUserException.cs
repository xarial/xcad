//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.Exceptions
{
    public class AppStartCancelledByUserException : Exception
    {
        public AppStartCancelledByUserException() : base("Application start is cancelled by user") 
        {
        }
    }
}
