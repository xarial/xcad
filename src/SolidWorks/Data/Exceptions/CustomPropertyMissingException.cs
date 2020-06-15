using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.Data.Exceptions
{
    public class CustomPropertyMissingException : Exception
    {
        public CustomPropertyMissingException(string name) : base($"'{name}' property doesn't exist. Use '{nameof(SwCustomPropertiesCollection.GetOrPreCreate)}' method instead to create new property") 
        {
        }
    }
}
