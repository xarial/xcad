//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
        /// Event for <see cref="OnConfigureServices(IXServiceCollection)"/>
        /// </summary>
        event ConfigureServicesDelegate ConfigureServices;

        /// <summary>
        /// Allows to configure services of the object
        /// </summary>
        /// <param name="collection">Collection of services</param>
        void OnConfigureServices(IXServiceCollection collection);
    }
}
