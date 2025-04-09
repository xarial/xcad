using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Exceptions
{
    /// <summary>
    /// Indicates that controlsof property page cannot be dynamically changed
    /// </summary>
    public class DynamicControlsNotSupportedException : NotSupportedException
    {
        /// <summary>
        /// Defautl constructor
        /// </summary>
        public DynamicControlsNotSupportedException() : base("Cannot create the control for changed items. This control does not allow dynamic changing of the items. For the dynamic items use the static items source property and initiate it with items") 
        {
        }
    }
}
