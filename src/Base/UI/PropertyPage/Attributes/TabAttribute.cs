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
        public Type ConstructorType { get; }

        /// <summary>
        /// Sets the current property as tab container
        /// </summary>
        public TabAttribute()
        {
            ConstructorType = typeof(ITabConstructor);
        }
    }
}