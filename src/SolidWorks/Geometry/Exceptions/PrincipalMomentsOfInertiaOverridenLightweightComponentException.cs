//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.Geometry.Exceptions
{
    public class PrincipalMomentsOfInertiaOverridenLightweightComponentException : NotSupportedException
    {
        internal PrincipalMomentsOfInertiaOverridenLightweightComponentException()
            : base($"Incorrect calculation of Principal Moments Of Intertia in SOLIDWORKS 2020 onwards for the overriden Moments of Inertia for lightweigth component")
        {
        }
    }
}
