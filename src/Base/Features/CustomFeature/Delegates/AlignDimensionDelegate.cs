//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Annotations;

namespace Xarial.XCad.Features.CustomFeature.Delegates
{
    /// <summary>
    /// Handler function to align specific dimension of <see cref="IXCustomFeatureDefinition{TParams}"></see> within the <see cref="IXCustomFeatureDefinition.OnRebuild(IXApplication, Documents.IXDocument, IXCustomFeature)"/>/>
    /// </summary>
    /// <typeparam name="TData">Type of the data</typeparam>
    /// <param name="paramName">Name of the parameter in the data model which corresponds to this dimension</param>
    /// <param name="dim">Dimension to align</param>
    public delegate void AlignDimensionDelegate<TData>(string paramName, IXDimension dim)
        where TData : class;
}