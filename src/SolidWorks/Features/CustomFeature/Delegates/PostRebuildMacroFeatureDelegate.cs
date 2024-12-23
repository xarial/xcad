//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature.Delegates
{
    /// <summary>
    /// Delegate of <see cref="SwMacroFeatureDefinition.PostRebuild"/> event
    /// </summary>
    /// <param name="app">Application</param>
    /// <param name="doc">Document</param>
    /// <param name="feat">Feature</param>
    public delegate void PostRebuildMacroFeatureDelegate(ISwApplication app, ISwDocument doc, ISwMacroFeature feat);

    /// <inheritdoc/>
    public delegate void PostRebuildMacroFeatureDelegate<TParams>(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feat)
        where TParams : class;
}
