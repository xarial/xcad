//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Features.CustomFeature.Delegates
{
    /// <summary>
    /// Control if the edit body should be hidden during the preview
    /// </summary>
    /// <param name="body">Body which is about to be hidden</param>
    /// <param name="data">Macro feature data</param>
    /// <param name="page">Macro feature page</param>
    /// <returns>True to hide body, false to kepe the body visible</returns>
    /// <remarks>usually edit body is hidden during the preview as it is replaced by the macro feature geometry
    /// In some cases user might need to perform multiple selections on edit body and thus hiding it preventing the further selections</remarks>
    public delegate bool ShouldHidePreviewEditBodyDelegate<TData, TPage>(IXBody body, TData data, TPage page)
            where TData : class
            where TPage : class;
}
