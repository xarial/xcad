using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.UI.PropertyPage.Base
{
    /// <summary>
    /// Represents the custom metadata which is used by controls
    /// </summary>
    public interface IMetadataAttribute : IAttribute
    {
        /// <summary>
        /// Tag of the metadata
        /// </summary>
        object Tag { get; }
    }
}
