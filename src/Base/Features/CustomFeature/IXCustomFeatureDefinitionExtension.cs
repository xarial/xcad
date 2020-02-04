//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Annotations;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Features.CustomFeature
{
    public static class IXCustomFeatureDefinitionExtension
    {
        public static void AlignRadialDimension<TParams>(this IXCustomFeatureDefinition<TParams> featDef, IXDimension dim, Point originPt, Vector normal)
            where TParams : class, new()
        {
            Vector dir = null;
            Vector extDir = null;

            var yVec = new Vector(0, 1, 0);

            if (normal.IsSame(yVec))
            {
                dir = new Vector(1, 0, 0);
            }
            else
            {
                dir = normal.Cross(yVec);
            }

            extDir = normal.Cross(dir);

            var endPt = CalculateEndPoint(dim, originPt, normal);

            featDef.AlignDimension(dim, new Point[] { originPt, endPt }, dir, extDir);
        }

        public static void AlignLinearDimension<TParams>(this IXCustomFeatureDefinition<TParams> featDef, IXDimension dim, Point originPt, Vector dir)
            where TParams : class, new()
        {
            var yVec = new Vector(0, 1, 0);

            Vector extDir;

            if (dir.IsSame(yVec))
            {
                extDir = new Vector(1, 0, 0);
            }
            else
            {
                extDir = yVec.Cross(dir);
            }

            var endPt = CalculateEndPoint(dim, originPt, dir);

            featDef.AlignDimension(dim, new Point[] { originPt, endPt }, dir, extDir);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TParams"></typeparam>
        /// <param name="featDef"></param>
        /// <param name="dim"></param>
        /// <param name="centerPt">Point at the center of the radiam dimension (fixed point)</param>
        /// <param name="refPt">Reference point of the radial dimension (fixed point)</param>
        /// <param name="rotVec">Vector, normal to the radial dimension extension line</param>
        public static void AlignAngularDimension<TParams>(this IXCustomFeatureDefinition<TParams> featDef,
            IXDimension dim, Point centerPt, Point refPt, Vector rotVec)
            where TParams : class, new()
        {
            var angle = dim.GetValue();

            var dirVec = new Vector(refPt.X - centerPt.X, refPt.Y - centerPt.Y, refPt.Z - centerPt.Z);

            var contLegLenth = dirVec.GetLength();

            var alignDir = rotVec.Cross(dirVec);

            var oppLegLength = contLegLenth * Math.Tan(angle);

            var midPt = refPt.Move(alignDir, oppLegLength);

            featDef.AlignDimension(dim, new Point[] { refPt, midPt, centerPt }, null, null);
        }

        private static Point CalculateEndPoint(IXDimension dim, Point startPt, Vector dir)
        {
            var length = dim.GetValue();
            return startPt.Move(dir, length);
        }
    }
}