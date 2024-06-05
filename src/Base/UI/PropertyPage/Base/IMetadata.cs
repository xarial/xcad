//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
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
        /// Notifies when metadata is changed
        /// </summary>
        event Action<IMetadata, object> Changed;

        /// <summary>
        /// User name of this metadata
        /// </summary>
        /// <remarks>The name is not neceserily needs to be unique</remarks>
        string Name { get; }

        /// <summary>
        /// Tag of this metadata
        /// </summary>
        object Tag { get; }

        /// <summary>
        /// Value associated with the metadata
        /// </summary>
        object Value { get; set; }
    }
}
