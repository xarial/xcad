//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Annotations;

namespace Xarial.XCad.Features.CustomFeature.Delegates
{
    public delegate void AlignDimensionDelegate<TData>(IXCustomFeatureDefinition<TData> def, string paramName, IXDimension dim)
        where TData : class, new();
}