//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Indicates that metadata should be attached to the control
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    public class AttachMetadataAttribute : Attribute, IHasMetadataAttribute
    {
        /// <inheritdoc/>
        public bool HasMetadata => true;

        /// <inheritdoc/>
        public object MetadataTag { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="tag">Tag of the metadata</param>
        public AttachMetadataAttribute(string tag) 
        {
            MetadataTag = tag;
        }
    }
}
