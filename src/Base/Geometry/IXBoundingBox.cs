using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Represents the bounding box of the geometrical object
    /// </summary>
    public interface IXBoundingBox : IXTransaction
    {
        /// <summary>
        /// Bounding box data
        /// </summary>
        Box3D Box { get; }

        /// <summary>
        /// Relative to transformation
        /// </summary>
        TransformMatrix RelativeTo { get; set; }

        /// <summary>
        /// True to calculate precise bounding box, false to calculate approximate bounding box
        /// </summary>
        bool Precise { get; set; }

        /// <summary>
        /// Scope of bodies to consider in this bounding box, null for all bodies
        /// </summary>
        IXBody[] Scope { get; set; }
    }

    /// <summary>
    /// Bounding box specific to the assembly
    /// </summary>
    public interface IXAssemblyBoundingBox : IXBoundingBox
    {
        /// <summary>
        /// Scope of components to consider in this bounding box, null for all components
        /// </summary>
        new IXComponent[] Scope { get; set; }

        /// <summary>
        /// Indicates to only consider visible components and bodies
        /// </summary>
        bool VisibleOnly { get; set; }
    }
}
