//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents an assembly document (composition of <see cref="IXPart"/> and other <see cref="IXAssembly"/>)
    /// </summary>
    public interface IXAssembly : IXDocument3D
    {
        /// <inheritdoc/>
        new IXAssemblyConfigurationRepository Configurations { get; }
    }
}