//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
    /// <remarks>This attribute is only applicable for <see cref="Enum">enum</see> types</remarks>
    public class OptionBoxAttribute : Attribute, ISpecificConstructorAttribute
    {
        /// <summary>
        /// Type of the constructor
        /// </summary>
        public Type ConstructorType { get; }

        /// <summary>
        /// Sets the current property as option box
        /// </summary>
        public OptionBoxAttribute()
        {
            ConstructorType = typeof(IOptionBoxConstructor);
        }
    }
}