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
