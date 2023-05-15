using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.UI.Commands.Attributes;

namespace Xarial.XCad.SolidWorks.UI.Commands.Attributes
{
    /// <summary>
    /// SOLIDWORKS specific context command menu spec
    /// </summary>
    public class SwContextMenuCommandItemInfoAttribute : ContextMenuCommandItemInfoAttribute
    {
        /// <summary>
        /// Selection type of the owner for this context menu
        /// </summary>
        public new swSelectType_e Owner { get; }

        /// <summary>
        /// Default construcotr
        /// </summary>
        /// <param name="owner">selection type of the owner for this context menu</param>
        public SwContextMenuCommandItemInfoAttribute(swSelectType_e owner) : base(null)
        {
            Owner = owner;
        }
    }
}
