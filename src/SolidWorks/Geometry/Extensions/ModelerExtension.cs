//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.SolidWorks.Geometry.Extensions
{
    internal static class ModelerExtension
    {
        internal static Curve CreateTrimmedLine(this IModeler modeler, Point startPt, Point endPt)
        {
            var line = modeler.CreateLine(startPt.ToArray(), (startPt - endPt).ToArray()) as Curve;
            line = line.CreateTrimmedCurve2(startPt.X, startPt.Y, startPt.Z, endPt.X, endPt.Y, endPt.Z);

            if (line == null)
            {
                throw new NullReferenceException("Failed to create line");
            }

            return line;
        }
    }
}
