using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Provides evaluation features to the see <see cref="IXDocument3D"/>
    /// </summary>
    public interface IXDocumentEvaluation
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
        /// Pre creates ray intersection calculator
        /// </summary>
        /// <returns>Rat intersection</returns>
        IXRayIntersection PreCreateRayIntersection();
    }

    /// <summary>
    /// Provides the specific evaluation for <see cref="IXAssembly"/>
    /// </summary>
    public interface IXAssemblyEvaluation : IXDocumentEvaluation
    {
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

        /// <summary>
        /// Pre creates ray intersection calculator for assembly
        /// </summary>
        /// <returns>Rat intersection</returns>
        new IXAssemblyRayIntersection PreCreateRayIntersection();
    }
}
