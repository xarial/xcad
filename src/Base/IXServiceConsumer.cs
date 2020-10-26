using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad
{
    /// <summary>
    /// Declares that this object consumes services
    /// </summary>
    public interface IXServiceConsumer
    {
        /// <summary>
        /// Allows to configure services of the object
        /// </summary>
        /// <param name="collection">Collection of services</param>
        void ConfigureServices(IXServiceCollection collection);
    }
}
