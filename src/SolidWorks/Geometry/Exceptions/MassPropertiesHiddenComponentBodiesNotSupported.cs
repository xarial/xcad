using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Exceptions
{
    public class MassPropertiesHiddenComponentBodiesNotSupported : NotSupportedException, IUserException
    {
        public MassPropertiesHiddenComponentBodiesNotSupported()
            : base("Input component contains hidden bodies. 'Visible Only' option is not supported in SOLIDWORKS 2019")
        {
        }
    }
}
