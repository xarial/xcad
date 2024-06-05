//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
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

        /// <inheritdoc/>
        public string Name { get; }

        /// <summary>
        /// Marks this property as metadata
        /// </summary>
        /// <param name="tag">Tag of the metadata</param>
        public MetadataAttribute(object tag)
        {
            Tag = tag;
        }

        /// <summary>
        /// Marks this property as metadata with custom user name
        /// </summary>
        /// <inheritdoc/>
        /// <param name="name">User name of the metadata</param>
        public MetadataAttribute(object tag, string name) : this(tag)
        {
            Name = name;
        }
    }
}