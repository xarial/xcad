//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Features.CustomFeature;

namespace Xarial.XCad.Features
{
    public interface IXFeatureRepository : IXRepository<IXFeature>
    {
        IXFeature this[string name] { get; }

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

        IXSketch2D PreCreate2DSketch();

        IXSketch3D PreCreate3DSketch();
    }
}