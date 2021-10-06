//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Exceptions
{
    internal class NotLoadedMassPropertyComponentException : NotSupportedException
    {
        internal NotLoadedMassPropertyComponentException(IXComponent comp) 
            : base($"Reference document of the component '{comp.Name}' must be loaded in order to access this mass property in SOLIDWORKS 2019 or older") 
        {
        }
    }
}
