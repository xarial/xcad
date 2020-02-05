using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature.Exceptions
{
    public class MacroFeatureDefinitionTypeMismatch : InvalidCastException
    {
        public MacroFeatureDefinitionTypeMismatch(Type defType, Type expectedType) 
            : base($"{defType.FullName} must inherit {expectedType.FullName}") 
        {
        }
    }
}
