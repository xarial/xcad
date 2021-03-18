using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Indicates that this group should have a check box
    /// </summary>
    public interface ICheckableGroupBoxAttribute : IAttribute
    {
    }

    /// <inheritdoc/>
    public class CheckableGroupBoxAttribute : Attribute, IHasMetadataAttribute, ICheckableGroupBoxAttribute
    {
        /// <inheritdoc/>
        public object MetadataTag { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="metadataTag">Reference to property which defines the toggle state</param>
        public CheckableGroupBoxAttribute(object metadataTag) 
        {
            MetadataTag = metadataTag;
        }
    }
}
