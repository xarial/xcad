using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Delegates
{
    /// <summary>
    /// Delegate for <see cref="IXServiceConsumer.ConfigureServices"/> event
    /// </summary>
    /// <param name="sender">Sender of this event</param>
    /// <param name="collection">Collection of services to configure</param>
    public delegate void ConfigureServicesDelegate(IXServiceConsumer sender, IXServiceCollection collection);
}
