//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the part document
    /// </summary>
    public interface IXPart : IXDocument3D
    {
        /// <inheritdoc/>
        new IXPartConfigurationRepository Configurations { get; }

        /// <summary>
        /// Material assigned to part
        /// </summary>
        IXMaterial Material { get; set; }

        /// <summary>
        /// Bodies in this part document
        /// </summary>
        IXBodyRepository Bodies { get; }
    }
}