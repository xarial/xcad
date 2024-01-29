//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SwDocumentManager.Exceptions
{
    /// <summary>
    /// Exception indicates that cut-lists cannot be exctrated for this version of the SOLIDWORKS file
    /// </summary>
    public class ConfigurationCutListIsNotSupported : NotSupportedException, IUserException
    {
        public ConfigurationCutListIsNotSupported() 
            : base("Cut-lists can only be extracted from the active configuration for files saved in 2018 or older") 
        {
        }
    }
}
