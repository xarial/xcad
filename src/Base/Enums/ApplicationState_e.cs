using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Enums
{
    [Flags]
    public enum ApplicationState_e
    {
        Default = 0,
        Hidden = 1,
        Background = 2,
        Silent = 4,
        Safe = 8
    }
}
