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
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Surfaces;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Geometry.Surfaces
{
    /// <summary>
    /// SOLIDWORKS-specific <see cref="IXSurface"/>
    /// </summary>
    public interface ISwSurface : IXSurface, ISwObject
    {
        /// <summary>
        /// Pointer to SOLIDWORKS surfaces
        /// </summary>
        ISurface Surface { get; }
    }

    internal abstract class SwSurface : SwObject, ISwSurface
    {
        public ISurface Surface { get; }

        public override object Dispatch => Surface;

        private readonly IMathUtility m_MathUtils;

        protected SwSurface(ISurface surface, SwDocument doc, SwApplication app) : base(surface, doc, app)
        {
            Surface = surface;
            m_MathUtils = app.Sw.IGetMathUtility();
        }

        public bool TryProjectPoint(Point point, Vector direction, out Point projectedPoint)
        {
            var dirVec = (MathVector)m_MathUtils.CreateVector(direction.ToArray());
            var startPt = (MathPoint)m_MathUtils.CreatePoint(point.ToArray());

            var resPt = Surface.GetProjectedPointOn(startPt, dirVec);

            if (resPt != null)
            {
                projectedPoint = new Point((double[])resPt.ArrayData);
                return true;
            }
            else
            {
                projectedPoint = null;
                return false;
            }
        }

        public Point FindClosestPoint(Point point)
            => new Point(((double[])Surface.GetClosestPointOn(point.X, point.Y, point.Z)).Take(3).ToArray());

        public Point CalculateLocation(double uParam, double vParam, out Vector normal)
        {
            var evalData = (double[])Surface.Evaluate(uParam, vParam, 1, 1);

            normal = new Vector(evalData.Skip(evalData.Length - 3).ToArray());

            return new Point(evalData.Take(3).ToArray());
        }

        public Vector CalculateNormalAtPoint(Point point)
        {
            if (point == null) 
            {
                throw new ArgumentNullException(nameof(point));
            }

            var evalData = (double[])Surface.EvaluateAtPoint(point.X, point.Y, point.Z);

            if (evalData != null)
            {
                return new Vector(evalData[0], evalData[1], evalData[2]);
            }
            else 
            {
                throw new NullReferenceException("Failed to evaluate surface at point. This can indicate that point does not lie on the surface");
            }
        }
    }
}
