//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Features.CustomFeature;

namespace Xarial.XCad.Features
{
    public static class IXFeatureRepositoryExtension
    {
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

        public static IXCustomFeature CreateCustomFeature<TDef>(this IXFeatureRepository feats)
            where TDef : IXCustomFeatureDefinition
        {
            var custFeat = feats.PreCreateCustomFeature();
            custFeat.DefinitionType = typeof(TDef);
            feats.Add(custFeat);

            return custFeat;
        }

        public static void CreateCustomFeature<TDef, TParams, TPage>(this IXFeatureRepository feats)
            where TParams : class, new()
            where TPage : class
            where TDef : class, IXCustomFeatureDefinition<TParams, TPage>, new()
            => feats.CreateCustomFeature<TDef, TParams, TPage>(new TParams());
    }
}