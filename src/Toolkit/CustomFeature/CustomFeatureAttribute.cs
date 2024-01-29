//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Features.CustomFeature;

namespace Xarial.XCad.Utils.CustomFeature
{
    /// <summary>
    /// Represents the custom attribute of the <see cref="IXCustomFeature"/>
    /// </summary>
    public class CustomFeatureAttribute
    {
        /// <summary>
        /// Name of the attribute
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Data type of the attribute
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Value of the attribute
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Attribute name</param>
        /// <param name="type">Attribute data type</param>
        /// <param name="value">Attribute value</param>
        public CustomFeatureAttribute(string name, Type type, object value)
        {
            Name = name;
            Type = type;
            Value = value;
        }
    }
}