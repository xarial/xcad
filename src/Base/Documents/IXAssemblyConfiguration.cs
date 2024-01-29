//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the configuration of the assembly
    /// </summary>
    public interface IXAssemblyConfiguration : IXConfiguration 
    {
        /// <summary>
        /// Components in this assembly configuration
        /// </summary>
        IXComponentRepository Components { get; }
    }
}