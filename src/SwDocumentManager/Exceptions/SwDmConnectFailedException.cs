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

namespace Xarial.XCad.SwDocumentManager.Exceptions
{
    public class SwDmConnectFailedException : Exception, IUserException
    {
        internal SwDmConnectFailedException(Exception ex)
            : base("Failed to connect to Document Manager API. Make sure that the specified license key is valid", ex)
        {
        }
    }
}
