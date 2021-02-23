using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Toolkit.PageBuilder.Exceptions
{
    public class DuplicateMetadataTagException : Exception
    {
        public DuplicateMetadataTagException(object tag) : base($"'{tag?.ToString()}' tag already assigned to metadata") 
        {
        }
    }
}
