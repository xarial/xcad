//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.UI.PropertyPage.Base
{
    /// <summary>
    /// Indicates that this data model creates controls dynamically
    /// </summary>
    /// <remarks>Implement this interface in the data model <see cref="IXPropertyPage{TDataModel}"/> and tag properties of data model with <see cref="Attributes.DynamicControlsAttribute"/></remarks>
    public interface IDynamicControlFactory
    {
        /// <summary>
        /// Handler of dynamic controls in the property page
        /// </summary>
        /// <param name="parent">Parent group</param>
        /// <param name="tag">Control tag assigned via <see cref="Attributes.DynamicControlsAttribute.Tag"/></param>
        /// <returns>Dynamic control descriptors</returns>
        IControlDescriptor[] CreateControls(IGroup parent, object tag);
    }
}
