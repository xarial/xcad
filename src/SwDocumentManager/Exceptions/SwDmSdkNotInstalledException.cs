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

namespace Xarial.XCad.SwDocumentManager.Exceptions
{
    public class SwDmSdkNotInstalledException : NullReferenceException, IUserException
    {
        internal SwDmSdkNotInstalledException() : base("SOLIDWORKS Document Manager SDK is not installed") 
        {
        }
    }
}
