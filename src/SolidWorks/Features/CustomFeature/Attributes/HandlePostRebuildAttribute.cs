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
