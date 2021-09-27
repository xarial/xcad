using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Exceptions
{
    /// <summary>
    /// IMassProperty API does not support option to specify to include or exclude hidden bodies or components (this option is always default to include hidden)
    /// This excepion indicates that mass cannot be calculated for the assembly which has hidden bodies while <see cref="XCad.Base.IEvaluation.VisibleOnly"/> is set to True
    /// </summary>
    public class MassPropertiesHiddenComponentBodiesNotSupported : NotSupportedException, IUserException
    {
        public MassPropertiesHiddenComponentBodiesNotSupported()
            : base("Input component contains hidden bodies. But 'Visible Bodies And Components Only' option is specified. This condition is not supported in SOLIDWORKS 2019")
        {
        }
    }
}
