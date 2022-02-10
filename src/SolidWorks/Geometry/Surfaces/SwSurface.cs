//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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
    public interface ISwSurface : IXSurface, ISwObject
    {
        ISurface Surface { get; }
    }

    internal abstract class SwSurface : SwObject, ISwSurface
    {
        public ISurface Surface { get; }

        public override object Dispatch => Surface;

        private readonly IMathUtility m_MathUtils;

        protected SwSurface(ISurface surface, ISwDocument doc, ISwApplication app) : base(surface, doc, app)
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
    }
}
