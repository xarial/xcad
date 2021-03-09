//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.UI.PropertyPage.Base
{
    /// <summary>
    /// Describes the control in this property page
    /// </summary>
    public interface IControlDescriptor
    {
        /// <summary>
        /// Display name of the control
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Tooltip of the control
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Unique name (id) of the control
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Icon of the control
        /// </summary>
        IXImage Icon { get; }

        /// <summary>
        /// Object type this control associated with
        /// </summary>
        Type DataType { get; }
        
        /// <summary>
        /// Gets the value from the object associated with the control
        /// </summary>
        /// <param name="context">Object context</param>
        /// <returns>Value</returns>
        object GetValue(object context);

        /// <summary>
        /// Sets the value to the context of the associated object
        /// </summary>
        /// <param name="context">Object context</param>
        /// <param name="value">Value to set</param>
        void SetValue(object context, object value);

        /// <summary>
        /// Atributes of this control
        /// </summary>
        IAttribute[] Attributes { get; }
    }
}
