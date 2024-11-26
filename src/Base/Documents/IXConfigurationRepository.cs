//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Documents.Delegates;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the collection of configurations in <see cref="IXDocument3D"/>
    /// </summary>
    public interface IXConfigurationRepository : IXRepository<IXConfiguration>
    {
        /// <summary>
        /// Fired when configuration is activated
        /// </summary>
        event ConfigurationActivatedDelegate ConfigurationActivated;

        /// <summary>
        /// Returns the currently active configuration or activates the specific configuration
        /// </summary>
        IXConfiguration Active { get; set; }

        /// <summary>
        /// Refreshes the configuration tree
        /// </summary>
        void Refresh();
    }
}
