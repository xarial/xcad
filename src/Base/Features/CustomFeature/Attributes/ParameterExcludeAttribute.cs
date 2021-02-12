using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Features.CustomFeature.Attributes
{
    /// <summary>
    /// Indicates that this property should not be considered as macro feature parameter
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ParameterExcludeAttribute : Attribute
    {
    }
}
