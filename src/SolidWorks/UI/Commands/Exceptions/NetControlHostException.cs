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
    public class NetControlHostException : Exception
    {
        public NetControlHostException(IntPtr handle) : base($"Failed to host .NET control (handle {handle}) in task pane") 
        {
        }
    }
}
