using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.Toolkit.PageBuilder.Exceptions
{
    public class MissingMetadataException : Exception
    {
        public MissingMetadataException(object tag, IControlDescriptor ctrlDesc) 
            : base($"Failed to find the metadata '{tag?.ToString()}' for {ctrlDesc.DisplayName}")
        {
        }
    }
}
