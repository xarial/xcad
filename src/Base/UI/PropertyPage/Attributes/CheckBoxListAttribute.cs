//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
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
    /// <remarks>This attribute is only applicable for flag <see cref="Enum">enum</see> types</remarks>
    public class CheckBoxListAttribute : Attribute, ISpecificConstructorAttribute
    {
        /// <summary>
        /// Type of the constructor
        /// </summary>
        public Type ConstructorType { get; }

        /// <summary>
        /// Sets the current property as check box list
        /// </summary>
        public CheckBoxListAttribute()
        {
            ConstructorType = typeof(ICheckBoxListConstructor);
        }
    }
}