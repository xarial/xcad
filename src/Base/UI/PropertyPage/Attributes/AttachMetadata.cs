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
    public class AttachMetadata : Attribute, IHasMetadataAttribute
    {
        public bool HasMetadata => true;

        public object MetadataTag { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="tag">Tag of the metadata</param>
        public AttachMetadata(string tag) 
        {
            MetadataTag = tag;
        }
    }
}
