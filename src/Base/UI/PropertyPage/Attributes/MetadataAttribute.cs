//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <inheritdoc/>
    public class MetadataAttribute : Attribute, IMetadataAttribute
    {
        /// <inheritdoc/>
        public object Tag { get; }

        public MetadataAttribute(object tag)
        {
            Tag = tag;
        }
    }
}