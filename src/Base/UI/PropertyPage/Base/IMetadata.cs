//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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
        /// Tag of this metadata
        /// </summary>
        object Tag { get; }

        /// <summary>
        /// Value associated with the metadata
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// Notifies when metadata is changed
        /// </summary>
        event Action<IMetadata, object> Changed;
    }
}
