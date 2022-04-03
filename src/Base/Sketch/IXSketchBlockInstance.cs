using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Sketch
{
    /// <summary>
    /// Represents an instance of <see cref="IXSketchBlockDefinition"/>
    /// </summary>
    public interface IXSketchBlockInstance : IXSketchEntity, IXFeature
    {
        /// <summary>
        /// Definition of this sketch block instance
        /// </summary>
        IXSketchBlockDefinition Definition { get; }

        /// <summary>
        /// Transformation of this sketch block instance regarding its defintion
        /// </summary>
        TransformMatrix Transform { get; }

        /// <summary>
        /// Entities of this sketch block definition
        /// </summary>
        IXSketchEntityRepository Entities { get; }
    }

    /// <summary>
    /// Additional methods of <see cref="IXSketchBlockInstance"/>
    /// </summary>
    public static class IXSketchBlockInstanceExtension 
    {
        /// <summary>
        /// Returns the total transform of this block, including parent block transforms
        /// </summary>
        /// <param name="skBlockInst"></param>
        /// <returns></returns>
        public static TransformMatrix GetTotalTransform(this IXSketchBlockInstance skBlockInst) 
        {
            var transform = TransformMatrix.Identity;

            while (skBlockInst != null) 
            {
                transform = transform.Multiply(skBlockInst.Transform);
                skBlockInst = skBlockInst.OwnerBlock;
            }

            return transform;
        }
    }
}
