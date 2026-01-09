//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.Toolkit.Exceptions
{
    internal class OpenHelpLinkException : Exception, IUserException
    {
        internal OpenHelpLinkException(string err) : base(err)
        {
        }

        internal OpenHelpLinkException(string err, Exception inner) : base(err, inner)
        {
        }
    }
}
