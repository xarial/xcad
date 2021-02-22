using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.UI.PropertyPage.Base
{
    /// <summary>
    /// Indicates that this binding uses metadata
    /// </summary>
    public interface IHasMetadataAttribute : IAttribute
    {
        /// <summary>
        /// Tag of metadata linked to <see cref="IMetadataAttribute.Tag"/>
        /// </summary>
        object MetadataTag { get; }
    }
}
