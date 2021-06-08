//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Provides additional options for the drop-down list box
    /// </summary>
    /// <remarks>Must be applied to the property of <see cref="Enum"/></remarks>
    public class ComboBoxOptionsAttribute : Attribute, IAttribute
    {
        /// <summary>
        /// Specific rendering style of the combobox
        /// </summary>
        public ComboBoxStyle_e Style { get; }

        /// <summary>
        /// Instructs to select the default value (if available) to avoid the control with deselected value on start
        /// </summary>
        public bool SelectDefaultValue { get; }

        /// <summary>
        /// Constructor for specifying style of combo box
        /// </summary>
        /// <param name="style">Specific style applied for combo box control</param>
        /// <param name="selectDefaultValue">Instructs to select the default value (if available) to avoid the control with deselected value on start</param>
        public ComboBoxOptionsAttribute(ComboBoxStyle_e style = 0, bool selectDefaultValue = false)
        {
            Style = style;
            SelectDefaultValue = selectDefaultValue;
        }
    }
}