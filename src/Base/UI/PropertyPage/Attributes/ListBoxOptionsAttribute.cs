//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Provides additional options for the list box
    /// </summary>
    /// <remarks>Must be applied to the property decorated with <see cref="ListBoxAttribute"/></remarks>
    public class ListBoxOptionsAttribute : Attribute, IAttribute
    {
        /// <summary>
        /// Specific rendering style of the listbox
        /// </summary>
        public ListBoxStyle_e Style { get; }
        
        /// <summary>
        /// Constructor for specifying style of list box
        /// </summary>
        /// <param name="style">Specific style applied for list box control</param>
        public ListBoxOptionsAttribute(ListBoxStyle_e style)
        {
            Style = style;
        }
    }
}