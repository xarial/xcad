using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Exceptions;

namespace SwAddInExample
{
    public class UserException : Exception, IUserException
    {
        public UserException(string msg) : base(msg) 
        {
        }
    }
}
