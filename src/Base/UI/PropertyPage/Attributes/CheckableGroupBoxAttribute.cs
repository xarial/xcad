﻿//*********************************************************************
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
    /// Indicates that this group should have a check box
    /// </summary>
    public interface ICheckableGroupBoxAttribute : IAttribute
    {
        /// <summary>
        /// Reference to property which defines the toggle state 
        /// </summary>
        object ToggleMetadataTag { get; }
    }

    /// <inheritdoc/>
    public class CheckableGroupBoxAttribute : Attribute, IHasMetadataAttribute, ICheckableGroupBoxAttribute
    {
        /// <inheritdoc/>
        public object LinkedMetadataTag => ToggleMetadataTag;

        /// <inheritdoc/>
        public object ToggleMetadataTag { get; }

        /// <inheritdoc/>
        public bool HasMetadata => true;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="toggleMetadataTag">Reference to property which defines the toggle state</param>
        public CheckableGroupBoxAttribute(object toggleMetadataTag)
        {
            ToggleMetadataTag = toggleMetadataTag;
        }
    }
}
