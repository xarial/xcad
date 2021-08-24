using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;

namespace Xarial.XCad.SolidWorks.Geometry.Exceptions
{
    public class NotLoadedMassPropertyComponentException : NullReferenceException
    {
        public NotLoadedMassPropertyComponentException(IXComponent comp) 
            : base($"Reference document of the component '{comp.Name}' must be loaded in order to access this mass property") 
        {
        }
    }
}
