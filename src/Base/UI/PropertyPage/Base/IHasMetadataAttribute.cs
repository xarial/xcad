//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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
        /// Tag of metadata linked to <see cref="IMetadataAttribute.Tag"/>
        /// </summary>
        object MetadataTag { get; }
    }
}
