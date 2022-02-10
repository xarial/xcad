//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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
