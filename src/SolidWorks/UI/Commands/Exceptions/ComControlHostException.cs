//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.UI.Commands.Exceptions
{
    public class ComControlHostException : Exception
    {
        public ComControlHostException(string progId) 
            : base($"Failed to create COM control from '{progId}'. Make sure that COM component is properly registered") 
        {
        }
    }
}
