//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.ComponentModel;
using Xarial.XCad.Reflection;

namespace Xarial.XCad.Base.Attributes
{
    /// <summary>
    /// Decorates the description for the element (e.g. command, user control, object etc.)
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class SummaryAttribute : DescriptionAttribute
    {
        /// <summary>
        /// Constructor for element summary
        /// </summary>
        /// <param name="description">Description of the element</param>
        public SummaryAttribute(string description) : base(description)
        {
        }

        /// <inheritdoc cref="SummaryAttribute(string)"/>
        /// <param name="resType">Type of the static class (usually Resources)</param>
        /// <param name="descriptionResName">Resource name of the string for display name</param>
        public SummaryAttribute(Type resType, string descriptionResName)
            : this(ResourceHelper.GetResource<string>(resType, descriptionResName))
        {
        }
    }
}