//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <inheritdoc/>
    [AttributeUsage(AttributeTargets.Property)]
    public class MetadataAttribute : Attribute, IMetadataAttribute
    {
        /// <inheritdoc/>
        public object Tag { get; }

        /// <summary>
        /// Marks this property as metadata
        /// </summary>
        /// <param name="tag">Tag of the metadata</param>
        public MetadataAttribute(object tag)
        {
            Tag = tag;
        }
    }
}