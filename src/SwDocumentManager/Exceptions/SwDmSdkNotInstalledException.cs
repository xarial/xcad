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
