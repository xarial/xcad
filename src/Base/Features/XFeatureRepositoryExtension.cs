//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Features.CustomFeature;

namespace Xarial.XCad.Features
{
    /// <summary>
    /// Additional methods for <see cref="IXFeatureRepository"/>
    /// </summary>
    public static class XFeatureRepositoryExtension
    {
        /// <summary>
        /// Creates custom feature with specified parameters
        /// </summary>
        /// <typeparam name="TDef">Definition</typeparam>
        /// <typeparam name="TParams">Paramteres</typeparam>
        /// <param name="feats">Feature repository</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance of the custom feature</returns>
        public static IXCustomFeature<TParams> CreateCustomFeature<TDef, TParams>(this IXFeatureRepository feats, TParams param)
            where TParams : class
            where TDef : IXCustomFeatureDefinition<TParams>
        {
            var custFeat = feats.PreCreateCustomFeature<TParams>();
            custFeat.DefinitionType = typeof(TDef);
            custFeat.Parameters = param;
            feats.Add(custFeat);

            return custFeat;
        }

        /// <summary>
        /// Creates parameterlsess custom feature
        /// </summary>
        /// <typeparam name="TDef">Defintion of the custom feature</typeparam>
        /// <param name="feats">Feature repository</param>
        /// <returns>Instance of the custom feature</returns>
        public static IXCustomFeature CreateCustomFeature<TDef>(this IXFeatureRepository feats)
            where TDef : IXCustomFeatureDefinition
        {
            var custFeat = feats.PreCreateCustomFeature();
            custFeat.DefinitionType = typeof(TDef);
            feats.Add(custFeat);

            return custFeat;
        }

        /// <summary>
        /// Starts the insertion of the custom feature with page editor
        /// </summary>
        /// <typeparam name="TDef">Defintion</typeparam>
        /// <typeparam name="TParams">Parameters</typeparam>
        /// <typeparam name="TPage">Page</typeparam>
        /// <param name="data">Default feature parameters</param>
        /// <param name="feats">Feature repository</param>
        public static void InsertCustomFeature<TDef, TParams, TPage>(this IXFeatureRepository feats, TParams data)
            where TParams : class
            where TPage : class
            where TDef : class, IXCustomFeatureDefinition<TParams, TPage>
            => feats.InsertCustomFeature(typeof(TDef), data);

        /// <inheritdoc cref="InsertCustomFeature{TDef, TParams, TPage}(IXFeatureRepository, TParams)"/>
        public static void InsertCustomFeature<TDef, TParams, TPage>(this IXFeatureRepository feats)
            where TParams : class, new()
            where TPage : class
            where TDef : class, IXCustomFeatureDefinition<TParams, TPage>, new()
            => InsertCustomFeature<TDef, TParams, TPage>(feats, new TParams());

        /// <summary>
        /// Creates a template for 2D sketch
        /// </summary>
        /// <returns>2D sketch template</returns>
        public static IXSketch2D PreCreate2DSketch(this IXFeatureRepository feats) => feats.PreCreate<IXSketch2D>();

        /// <summary>
        /// Creates a template for 3D sketch
        /// </summary>
        /// <returns>2D sketch template</returns>
        public static IXSketch3D PreCreate3DSketch(this IXFeatureRepository feats) => feats.PreCreate<IXSketch3D>();

        /// <summary>
        /// Pre-creates custom feature
        /// </summary>
        /// <returns>Instance of custom feature</returns>
        public static IXCustomFeature PreCreateCustomFeature(this IXFeatureRepository feats) => feats.PreCreate<IXCustomFeature>();

        /// <summary>
        /// Pre-creates dumb body feature
        /// </summary>
        /// <returns>Instance of dumb body feature</returns>
        public static IXDumbBody PreCreateDumbBody(this IXFeatureRepository feats) => feats.PreCreate<IXDumbBody>();

        /// <summary>
        /// Pre-creates custom feature with specific parameters
        /// </summary>
        /// <typeparam name="TParams">Type of parameters managed by this custom feature</typeparam>
        /// <returns>Instance of custom feature</returns>
        public static IXCustomFeature<TParams> PreCreateCustomFeature<TParams>(this IXFeatureRepository feats)
            where TParams : class
            => feats.PreCreate<IXCustomFeature<TParams>>();
    }
}