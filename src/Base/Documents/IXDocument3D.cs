//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents 3D document (assembly or part)
    /// </summary>
    public interface IXDocument3D : IXDocument, IXObjectContainer
    {
        /// <summary>
        /// Pre creates the 3D bounding box of the document
        /// </summary>
        /// <returns>Bounding box</returns>
        IXBoundingBox PreCreateBoundingBox();

        /// <summary>
        /// Pre creates the mass property evaluator for the document
        /// </summary>
        /// <returns>Mass property</returns>
        IXMassProperty PreCreateMassProperty();

        /// <summary>
        /// Returns 3D views collection
        /// </summary>
        new IXModelView3DRepository ModelViews { get; }

        /// <summary>
        /// Returns configurations of this document
        /// </summary>
        IXConfigurationRepository Configurations { get; }
    }
}