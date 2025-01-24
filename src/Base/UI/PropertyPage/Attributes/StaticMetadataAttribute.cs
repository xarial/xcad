//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Indicates that static metadata should be attached to the control
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    public class StaticMetadataAttribute : Attribute, IStaticMetadataAttribute
    {
        /// <inheritdoc/>
        public object StaticValue { get; set; }

        /// <inheritdoc/>
        public string Name { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public StaticMetadataAttribute()
        {
        }

        /// <summary>
        /// Constructor setting the static value of the metadata
        /// </summary>
        /// <param name="staticValue"></param>
        public StaticMetadataAttribute(object staticValue)
        {
            StaticValue = staticValue;
        }

        /// <inheritdoc/>
        /// <param name="name"></param>
        public StaticMetadataAttribute(object staticValue, string name) : this(staticValue)
        {
            Name = name;
        }
    }
}
