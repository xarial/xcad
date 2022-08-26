//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Utils
{
    internal static class Numeric
    {
        internal const double DEFAULT_NUMERIC_TOLERANCE = 1E-12;
        internal const double DEFAULT_LENGTH_TOLERANCE = 1E-8;
        internal const double DEFAULT_ANGLE_TOLERANCE = 1E-6;

        internal static bool Compare(double d1, double d2, double tol = DEFAULT_NUMERIC_TOLERANCE) => Math.Abs(d1 - d2) < tol;
    }
}
