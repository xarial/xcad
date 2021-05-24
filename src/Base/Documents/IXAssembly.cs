//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents an assembly document (composition of <see cref="IXPart"/> and other <see cref="IXAssembly"/>)
    /// </summary>
    public interface IXAssembly : IXDocument3D
    {
        /// <inheritdoc/>
        new IXAssemblyConfigurationRepository Configurations { get; }

        /// <summary>
        /// Pre creates the 3D bounding box of the assembly
        /// </summary>
        /// <returns>Bounding box</returns>
        new IXAssemblyBoundingBox PreCreateBoundingBox();

        /// <summary>
        /// Pre creates mass properties of the assembly
        /// </summary>
        /// <returns>Mass property</returns>
        new IXAssemblyMassProperty PreCreateMassProperty();
    }
}