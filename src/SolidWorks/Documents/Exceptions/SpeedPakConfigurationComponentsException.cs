//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SolidWorks.Documents.Exceptions
{
    public class SpeedPakConfigurationComponentsException : Exception, IUserException
    {
        public SpeedPakConfigurationComponentsException() : base("Components cannot be extracted from the SpeedPak configuration")
        {
        }
    }
}
