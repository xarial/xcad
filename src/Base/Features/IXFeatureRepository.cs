//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Features.CustomFeature;

namespace Xarial.XCad.Features
{
    /// <summary>
    /// Represents collection of features in the document
    /// </summary>
    public interface IXFeatureRepository : IXRepository<IXFeature>
    {
        /// <summary>
        /// Pre-creates custom feature
        /// </summary>
        /// <returns>Instance of custom feature</returns>
        IXCustomFeature PreCreateCustomFeature();

        /// <summary>
        /// Pre-creates custom feature with specific parameters
        /// </summary>
        /// <typeparam name="TParams">Type of parameters managed by this custom feature</typeparam>
        /// <returns>Instance of custom feature</returns>
        IXCustomFeature<TParams> PreCreateCustomFeature<TParams>()
            where TParams : class, new();

        /// <summary>
        /// Creates a custom feature with built-in editor for the property page
        /// </summary>
        /// <typeparam name="TDef">Definition of the custom feature</typeparam>
        /// <typeparam name="TParams">Type which defines the data structure of the custom feature</typeparam>
        /// <typeparam name="TPage">Type which defines the data model for the property page</typeparam>
        void CreateCustomFeature<TDef, TParams, TPage>()
            where TParams : class, new()
            where TPage : class, new()
            where TDef : class, IXCustomFeatureDefinition<TParams, TPage>, new();

        /// <summary>
        /// Creates a template for 2D sketch
        /// </summary>
        /// <returns>2D sketch template</returns>
        IXSketch2D PreCreate2DSketch();

        /// <summary>
        /// Creates a template for 3D sketch
        /// </summary>
        /// <returns>2D sketch template</returns>
        IXSketch3D PreCreate3DSketch();
    }
}