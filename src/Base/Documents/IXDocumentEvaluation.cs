//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Evaluation;

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
        /// <returns>Ray intersection</returns>
        IXRayIntersection PreCreateRayIntersection();

        /// <summary>
        /// Pre creates a geometry tessellation
        /// </summary>
        /// <returns>Tesselation</returns>
        IXTessellation PreCreateTessellation();

        /// <summary>
        /// Pre creates collision detection utility
        /// </summary>
        /// <returns>Collision detection</returns>
        IXCollisionDetection PreCreateCollisionDetection();

        /// <summary>
        /// Pre creates measure utility
        /// </summary>
        /// <returns>Measure utility</returns>
        IXMeasure PreCreateMeasure();
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
        /// <returns>Ray intersection</returns>
        new IXAssemblyRayIntersection PreCreateRayIntersection();

        /// <summary>
        /// Pre creates a geometry tessellation for assembly
        /// </summary>
        /// <returns>Tesselation</returns>
        new IXAssemblyTessellation PreCreateTessellation();

        /// <summary>
        /// Pre creates collision detection utility for assembly
        /// </summary>
        /// <returns>Collision detection</returns>
        new IXAssemblyCollisionDetection PreCreateCollisionDetection();
    }
}
