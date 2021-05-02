//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the collection of configurations in <see cref="IXAssembly"/>
    /// </summary>
    public interface IXAssemblyConfigurationRepository : IXConfigurationRepository, IXRepository<IXAssemblyConfiguration> 
    {
        /// <inheritdoc/>
        new IXAssemblyConfiguration Active { get; set; }

        /// <inheritdoc/>
        new IXAssemblyConfiguration PreCreate();
    }
}
