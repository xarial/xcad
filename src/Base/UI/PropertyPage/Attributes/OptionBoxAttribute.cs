//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    public interface IOptionBoxConstructor
    {
    }

    /// <summary>
    /// Attribute indicates that current property should be rendered as option box
    /// </summary>
    /// <remarks>This attribute is only applicable for <see cref="Enum">enum</see> types</remarks>
    public class OptionBoxAttribute : Attribute, ISpecificConstructorAttribute
    {
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