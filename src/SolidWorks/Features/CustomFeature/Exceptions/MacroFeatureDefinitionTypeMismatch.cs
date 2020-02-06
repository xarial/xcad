//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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
