//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Drawing;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Constructor of check box-lis
    /// </summary>
    public interface ICheckBoxListConstructor
    {
    }

    /// <summary>
    /// Attribute indicates that current property should be rendered as option box
    /// </summary>
    /// <remarks>This attribute is only applicable for flag <see cref="Enum">enum</see> types or <see cref="System.Collections.IList"/> if items source is specified</remarks>
    public class CheckBoxListAttribute : ItemsSourceControlAttribute, ISpecificConstructorAttribute
    {
        /// <summary>
        /// Type of the constructor
        /// </summary>
        public Type ConstructorType { get; } = typeof(ICheckBoxListConstructor);

        /// <summary>
        /// Sets the current property as check box list
        /// </summary>
        /// <remarks>Use this constructor on the flag <see cref="Enum"/> to render enum as group of check-boxes</remarks>
        public CheckBoxListAttribute()
        {
        }

        /// <inheritdoc/>
        public CheckBoxListAttribute(Type customItemsProviderType, params object[] dependencies) : base(customItemsProviderType, dependencies)
        {
        }

        /// <inheritdoc/>
        public CheckBoxListAttribute(params object[] items) : base(items)
        {
        }
    }
}