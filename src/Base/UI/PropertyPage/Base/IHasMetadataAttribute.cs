//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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
        /// True if the referenced property has metadata
        /// </summary>
        bool HasMetadata { get; }

        /// <summary>
        /// Tag of metadata linked to <see cref="IMetadataAttribute.Tag"/>
        /// </summary>
        object LinkedMetadataTag { get; }

        /// <summary>
        /// Static value of the metadata
        /// </summary>
        object StaticValue { get; }
    }
}
