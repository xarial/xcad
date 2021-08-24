//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature.Attributes
{
    /// <summary>
    /// Indicates that <see cref="SwMacroFeatureDefinition"/> should handle the <see cref="SwMacroFeatureDefinition.OnPostRebuild(ISwApplication, Documents.ISwDocument, ISwMacroFeature)"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class HandlePostRebuildAttribute : Attribute
    {
    }
}
