using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.SolidWorks.UI;

namespace Xarial.XCad.SolidWorks.Services
{
    public interface ICalloutHandlerProvider
    {
        SwCalloutBaseHandler CreateHandler(ISldWorks app);
    }
}
