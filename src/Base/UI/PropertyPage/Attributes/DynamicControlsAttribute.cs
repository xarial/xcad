//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Extensions;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Indicates that this property provides dynamic control for property page
    /// </summary>
    public class DynamicControlsAttribute : Attribute
    {
        /// <summary>
        /// Type of the control factory
        /// </summary>
        /// <remarks>Must implement <see cref="IDynamicControlFactory"/></remarks>
        public Type FactoryType { get; }

        /// <summary>
        /// User tag
        /// </summary>
        public object Tag { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="factoryType">Type of control factory. Must implement <see cref="IDynamicControlFactory"/></param>
        public DynamicControlsAttribute(Type factoryType) : this(factoryType, null)
        {
        }

        /// <summary>
        /// Constructor with tag
        /// </summary>
        /// <param name="factoryType">Type of control factory. Must implement <see cref="IDynamicControlFactory"/></param>
        /// <param name="tag">Tag to associate with dynamic controls</param>
        public DynamicControlsAttribute(Type factoryType, object tag)
        {
            if (!typeof(IDynamicControlFactory).IsAssignableFrom(factoryType)) 
            {
                throw new InvalidCastException($"'{factoryType.FullName}' must implement '{typeof(IDynamicControlFactory).FullName}'");
            }

            FactoryType = factoryType;
            Tag = tag;
        }
    }
}
