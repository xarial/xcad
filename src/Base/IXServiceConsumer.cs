//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Delegates;

namespace Xarial.XCad
{
    /// <summary>
    /// Declares that this object consumes services
    /// </summary>
    public interface IXServiceConsumer
    {
        /// <summary>
        /// Event to configure services/>
        /// </summary>
        /// <remarks>This event may not be fired if ConfigureServices method is available is marked as virtual method and it is overridden in the derived class</remarks>
        event ConfigureServicesDelegate ConfigureServices;
    }
}
