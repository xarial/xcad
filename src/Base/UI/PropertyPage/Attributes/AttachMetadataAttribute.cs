//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
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
        public object LinkedMetadataTag { get; set; }

        /// <inheritdoc/>
        public object StaticValue { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public AttachMetadataAttribute() 
        {
        }

        /// <summary>
        /// Constructor setting the link to metadata
        /// </summary>
        /// <param name="linkedMetadataTag">Tag of the metadata</param>
        public AttachMetadataAttribute(object linkedMetadataTag) 
        {
            LinkedMetadataTag = linkedMetadataTag;
        }
    }
}
