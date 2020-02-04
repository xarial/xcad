//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry;

namespace Xarial.XCad.Features.CustomFeature.Delegates
{
    public delegate IXBody[] CreateGeometryDelegate<TData>(IXCustomFeatureDefinition def, TData data,
        bool isPreview, out AlignDimensionDelegate<TData> alignDim)
        where TData : class, new();
}