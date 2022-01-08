//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Constructor of text block
    /// </summary>
    public interface ITextBlockConstructor
    {
    }

    /// <summary>
    /// Attribute indicates that current property should be rendered as text block
    /// </summary>
    /// <remarks>This attribute is only applicable for <see cref="Enum">enum</see> types</remarks>
    public class TextBlockAttribute : Attribute, ISpecificConstructorAttribute
    {
        /// <summary>
        /// Type of the constructor
        /// </summary>
        public Type ConstructorType { get; }

        /// <summary>
        /// Sets the current property as text box
        /// </summary>
        public TextBlockAttribute()
        {
            ConstructorType = typeof(ITextBlockConstructor);
        }
    }
}