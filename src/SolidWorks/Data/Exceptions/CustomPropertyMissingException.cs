//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.Data.Exceptions
{
    public class CustomPropertyMissingException : Exception
    {
        public CustomPropertyMissingException(string name) 
            : base($"'{name}' property doesn't exist. Use '{nameof(SwCustomPropertiesCollection.PreCreate)}' method instead to create new property") 
        {
        }
    }
}
