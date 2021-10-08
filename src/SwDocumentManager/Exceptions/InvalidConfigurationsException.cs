//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SwDocumentManager.Exceptions
{
    public class InvalidConfigurationsException : Exception, IUserException
    {
        public SwDMConfigurationError Error { get; }

        private static string GetError(SwDMConfigurationError err) 
        {
            switch (err) 
            {
                case SwDMConfigurationError.SwDMConfigurationError_ComObjectDisconnected:
                    return "Document has been closed";

                case SwDMConfigurationError.SwDMConfigurationError_DataMissing:
                    return "Configuration data missing";

                case SwDMConfigurationError.SwDMConfigurationError_NameNotFound:
                    return "Configuration name not found";

                case SwDMConfigurationError.SwDMConfigurationError_RequiredArgumentNull:
                    return "Required argument is null";

                case SwDMConfigurationError.SwDMConfigurationError_Unknown:
                    return "Unknown error";

                default:
                    return "Generic error";
            }
        }

        internal InvalidConfigurationsException(SwDMConfigurationError err) 
            : base(GetError(err))
        {
            Error = err;
        }
    }
}
