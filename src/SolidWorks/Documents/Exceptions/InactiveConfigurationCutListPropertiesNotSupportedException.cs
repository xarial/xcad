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
