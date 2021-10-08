using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.Geometry.Exceptions
{
    public class PrincipalAxesOfInertiaOverridenLightweightComponentException : NotSupportedException
    {
        internal PrincipalAxesOfInertiaOverridenLightweightComponentException()
            : base($"Incorrect calculation of Principal Axes Of Intertia for in SOLIDWORKS 2020 onwards for the overriden Moments of Inertia")
        {
        }
    }
}
