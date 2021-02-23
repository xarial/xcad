using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.UI.PropertyPage.Base
{
    /// <summary>
    /// Represents the custom metadata which is attached to binding
    /// </summary>
    public interface IMetadata
    {
        /// <summary>
        /// Notifies when metadata is changed
        /// </summary>
        event Action<IMetadata, object> Changed;
    }
}
