using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SwDocumentManager.Exceptions
{
    public class SpeedPakConfigurationComponentsException : Exception, IUserException
    {
        public SpeedPakConfigurationComponentsException() : base("Components cannot be extracted from the SpeedPak configuration")
        {
        }
    }
}
