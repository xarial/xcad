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
    /// Constructor of option box
    /// </summary>
    public interface IOptionBoxConstructor
    {
    }

    /// <summary>
    /// Attribute indicates that current property should be rendered as option box
    /// </summary>
    /// <remarks>This attribute is only applicable for <see cref="Enum">enum</see> types or <see cref="System.Collections.IList"/> if items source is specified</remarks>
    public class OptionBoxAttribute : ItemsSourceControlAttribute, ISpecificConstructorAttribute
    {
        /// <summary>
        /// Type of the constructor
        /// </summary>
        public Type ConstructorType { get; } = typeof(IOptionBoxConstructor);

        /// <summary>
        /// Sets the current property as option box
        /// </summary>
        /// <remarks>Use this constructor on the <see cref="Enum"/> to render enum as group of check-boxes</remarks>
        public OptionBoxAttribute()
        {
        }

        /// <inheritdoc/>
        public OptionBoxAttribute(Type customItemsProviderType, params object[] dependencies) : base(customItemsProviderType, dependencies)
        {
        }

        /// <inheritdoc/>
        public OptionBoxAttribute(params object[] items) : base(items)
        {
        }
    }
}