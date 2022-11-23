//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Inventor.Enums
{
    [Flags]
    public enum StartApplicationConnectStrategy_e
    {
        Default = 0,
        AllowCreatingTempTokenDocuments = 1,
        WaitUntilFullyLoaded
    }
}
