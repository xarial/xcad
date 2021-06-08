﻿using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SwDocumentManager.Exceptions
{
    public class ConfigurationSpecificCutListPropertiesWriteNotSupportedException : NotSupportedException, IUserException
    {
        public ConfigurationSpecificCutListPropertiesWriteNotSupportedException()
            : base("Modifying configuration specific cut-list properties is not supported. Instead modify the properties in active configuration only")
        {
        }
    }
}
