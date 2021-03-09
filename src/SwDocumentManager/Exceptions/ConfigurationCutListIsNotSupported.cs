//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SwDocumentManager.Exceptions
{
    public class ConfigurationCutListIsNotSupported : NotSupportedException, IUserException
    {
        public ConfigurationCutListIsNotSupported() : base("Cut-lists can only be extracted from the active configuration for files saved in 2018 or older") 
        {
        }
    }
}
