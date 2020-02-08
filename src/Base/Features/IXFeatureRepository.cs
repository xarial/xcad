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
        IXCustomFeature<TParams> PreCreateCustomFeature<TParams>()
            where TParams : class, new();

        void CreateCustomFeature<TDef, TParams, TPage>()
            where TParams : class, new()
            where TPage : class, new()
            where TDef : class, IXCustomFeatureDefinition<TParams, TPage>, new();

        IXSketch2D PreCreate2DSketch();

        IXSketch3D PreCreate3DSketch();
    }
}