//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    public interface ICustomControlConstructor 
    {
    }

    /// <summary>
    /// Indicates that this propery should be rendered as a custom control
    /// </summary>
    public class CustomControlAttribute : Attribute, ISpecificConstructorAttribute, IHasMetadataAttribute
    {
        /// <inheritdoc/>
        public Type ConstructorType { get; }
        
        /// <inheritdoc/>
        public Type ControlType { get; }

        /// <inheritdoc/>
        public bool HasMetadata { get; }

        /// <inheritdoc/>
        public object LinkedMetadataTag { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="controlType">Control type</param>
        /// <remarks>Implement <see cref="IXCustomControl"/> for additional configuration</remarks>
        public CustomControlAttribute(Type controlType) 
        {
            ConstructorType = typeof(ICustomControlConstructor);
            ControlType = controlType;

            HasMetadata = false;
        }

        /// <inheritdoc/>
        /// <param name="metadata">Metadata to load into control</param>
        public CustomControlAttribute(Type controlType, object metadata) : this(controlType)
        {
            LinkedMetadataTag = metadata;
            HasMetadata = true;
        }
    }
}
