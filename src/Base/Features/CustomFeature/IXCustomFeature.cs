//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Features.CustomFeature
{
    /// <summary>
    /// Instance of the custom feature
    /// </summary>
    public interface IXCustomFeature : IXFeature
    {
        /// <summary>
        /// Type of the definition of this custom feature
        /// </summary>
        Type DefinitionType { get; set; }

        /// <summary>
        /// Transformation of this feature
        /// </summary>
        /// <remarks>This is useful when the feature is inserted in the context of the assembly</remarks>
        TransformMatrix TargetTransformation { get; }

        /// <summary>
        /// Referenced configuration
        /// </summary>
        IXConfiguration Configuration { get; }
    }

    /// <summary>
    /// Instance of the custom feature with parameters
    /// </summary>
    /// <typeparam name="TParams">Parameters data model</typeparam>
    public interface IXCustomFeature`<TParams>` : IXCustomFeature
        where TParams : class
    {
        /// <summary>
        /// Parameters of this feature
        /// </summary>
        TParams Parameters { get; set; }

        /// <summary>
        /// Gets the transformation matrix of the specified entity of the macro feature
        /// </summary>
        /// <param name="entity">Entity to get the transformation from</param>
        /// <returns>Entity transformation matrix</returns>
        /// <remarks>Entity is a selection object which is specified in the <see cref="Parameters"/></remarks>
        TransformMatrix GetEntityTransformation(IXSelObject entity);
    }

    /// <summary>
    /// Additional methods for <see cref="IXCustomFeature"/>
    /// </summary>
    public static class XCustomFeatureExtension
    {
        /// <summary>
        /// Gets the actual transformation of the entity in case of the in-context editing
        /// </summary>
        /// <typeparam name="TParams">Parameters</typeparam>
        /// <param name="feat">Custom feature</param>
        /// <param name="entity">Entity</param>
        /// <returns>Total transform</returns>
        /// <remarks>Use this method to transform the coordinates and vectors from the selection entities in the parameters to the custom feature target</remarks>
        public static TransformMatrix GetEntityToTargetTransformation`<TParams>`(this IXCustomFeature`<TParams>` feat, IXSelObject entity)
            where TParams : class
        {
            var entTransform = feat.GetEntityTransformation(entity);
            var targetTransform = feat.TargetTransformation.Inverse();

            return entTransform* targetTransform;
        }
    }
}