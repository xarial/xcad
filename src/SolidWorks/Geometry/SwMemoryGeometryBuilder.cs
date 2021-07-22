//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Surfaces;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Geometry.Surfaces;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit.Data;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwMemoryGeometryBuilder : IXMemoryGeometryBuilder
    {
    }

    internal class SwMemoryGeometryBuilder : ISwMemoryGeometryBuilder
    {
        public IXWireGeometryBuilder WireBuilder { get; }
        public IXSheetGeometryBuilder SheetBuilder { get; }
        public IXSolidGeometryBuilder SolidBuilder { get; }

        private readonly IModeler m_Modeler;
        private readonly IMathUtility m_MathUtils;

        internal SwMemoryGeometryBuilder(ISwApplication app, IMemoryGeometryBuilderDocumentProvider geomBuilderDocsProvider) 
        {
            m_MathUtils = app.Sw.IGetMathUtility();
            m_Modeler = app.Sw.IGetModeler();

            WireBuilder = new SwMemoryWireGeometryBuilder(m_MathUtils, m_Modeler);
            SheetBuilder = new SwMemorySheetGeometryBuilder(m_MathUtils, m_Modeler);
            SolidBuilder = new SwMemorySolidGeometryBuilder(app, geomBuilderDocsProvider);
        }

        public IXBody DeserializeBody(Stream stream)
        {
            var comStr = new StreamWrapper(stream);
            var body = (IBody2)m_Modeler.Restore(comStr);
            return SwObjectFactory.FromDispatch<ISwTempBody>(body, null);
        }

        public void SerializeBody(IXBody body, Stream stream)
        {
            var comStr = new StreamWrapper(stream);
            ((SwBody)body).Body.Save(comStr);
        }

        public IXRegion CreateRegionFromSegments(params IXSegment[] segments)
            => new SwRegion(segments);

        public bool TryProjectPoint(IXFace face, Point point, Vector direction, out Point projectedPoint)
        {
            var swFace = ((ISwFace)face).Face;
            var dirVec = (MathVector)m_MathUtils.CreateVector(direction.ToArray());
            var startPt = (MathPoint)m_MathUtils.CreatePoint(point.ToArray());

            var resPt = swFace.GetProjectedPointOn(startPt, dirVec);

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

        public bool TryProjectPoint(IXSurface surface, Point point, Vector direction, out Point projectedPoint)
        {
            var swSurf = ((ISwSurface)surface).Surface;
            var dirVec = (MathVector)m_MathUtils.CreateVector(direction.ToArray());
            var startPt = (MathPoint)m_MathUtils.CreatePoint(point.ToArray());

            var resPt = swSurf.GetProjectedPointOn(startPt, dirVec);

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

        public Point FindClosestPoint(IXFace face, Point point)
            => new Point(((double[])((ISwFace)face).Face.GetClosestPointOn(point.X, point.Y, point.Z)).Take(3).ToArray());

        public Point FindClosestPoint(IXSurface surface, Point point)
            => new Point(((double[])((ISwSurface)surface).Surface.GetClosestPointOn(point.X, point.Y, point.Z)).Take(3).ToArray());
    }
}
