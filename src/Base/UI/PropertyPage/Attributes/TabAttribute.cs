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
    public interface ITabConstructor
    {
    }

    /// <summary>
    /// Attribute indicates that current property or class should be rendered as tab box
    /// </summary>
    /// <remarks>This attribute is only applicable for complex types which contain nested properties</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class TabAttribute : Attribute, ISpecificConstructorAttribute
    {
        /// <inheritdoc/>
        public Type ConstructorType { get; }

        /// <summary>
        /// Name of the function within the tab class which handles the tab click
        /// </summary>
        /// <remarks>This can be either private or public void function with no parameters</remarks>
        public string ClickHandlerFunctionName { get; }

        /// <summary>
        /// Sets the current property as tab container
        /// </summary>
        public TabAttribute()
        {
            ConstructorType = typeof(ITabConstructor);
        }

        /// <summary>
        /// Sets the current property as tab container and assigns the click handler
        /// </summary>
        /// <param name="clickHandlerFuncName">Name of hte click handler function</param>
        public TabAttribute(string clickHandlerFuncName) : this()
        {
            ClickHandlerFunctionName = clickHandlerFuncName;
        }
    }
}