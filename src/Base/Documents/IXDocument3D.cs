//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents 3D document (assembly or part)
    /// </summary>
    public interface IXDocument3D : IXDocument, IXObjectContainer
    {
        /// <summary>
        /// Extracts the 3D bounding box of the document parallel to XYZ coordinate system
        /// </summary>
        /// <returns>Bounding box</returns>
        Box3D CalculateBoundingBox();

        /// <summary>
        /// Returns views collection
        /// </summary>
        IXModelViewRepository ModelViews { get; }

        /// <summary>
        /// Returns configurations of this document
        /// </summary>
        IXConfigurationRepository Configurations { get; }
    }
}