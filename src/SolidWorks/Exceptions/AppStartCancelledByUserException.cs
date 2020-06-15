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
