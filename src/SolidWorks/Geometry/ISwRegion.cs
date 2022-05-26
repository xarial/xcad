//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Sketch;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwRegion : IXRegion
    {
        new ISwCurve[] Boundary { get; }
    }

    public interface ISwPlanarRegion : ISwRegion, IXPlanarRegion
    {
        ISwTempPlanarSheetBody PlanarSheetBody { get; }
    }

    internal class SwPlanarRegion : ISwPlanarRegion
    {
        public IXSegment[] Boundary { get; }

        public Plane Plane
        {
            get
            {
                Plane plane = null;

                var firstCurve = Boundary.FirstOrDefault() as SwCurve;

                if (firstCurve?.TryGetPlane(out plane) == true)
                {
                    return plane;
                }
                else
                {
                    //TODO: check if not colinear
                    //TODO: check if all on the same plane
                    //TODO: fix if a single curve

                    var refVec1 = Boundary[0].EndPoint.Coordinate - Boundary[0].StartPoint.Coordinate;
                    var refVec2 = Boundary[1].EndPoint.Coordinate - Boundary[1].StartPoint.Coordinate;
                    var normVec = refVec1.Cross(refVec2);

                    return new Plane(Boundary.First().StartPoint.Coordinate, normVec, refVec1);
                }
            }
        }

        ISwCurve[] ISwRegion.Boundary => m_LazyBoundary.Value;

        public ISwTempPlanarSheetBody PlanarSheetBody => this.ToPlanarSheetBody(m_GeomBuilder);

        private readonly Lazy<ISwCurve[]> m_LazyBoundary;

        private readonly ISwMemoryGeometryBuilder m_GeomBuilder;

        internal SwPlanarRegion(IXSegment[] boundary, ISwMemoryGeometryBuilder geomBuilder)
        {
            m_GeomBuilder = geomBuilder;
            Boundary = boundary;

            m_LazyBoundary = new Lazy<ISwCurve[]>(() =>
            {
                var res = new List<ISwCurve>();

                foreach (var bound in boundary) 
                {
                    switch (bound) 
                    {
                        case ISwCurve curve:
                            res.Add(curve);
                            break;

                        case ISwEdge edge:
                            res.Add(edge.Definition);
                            break;

                        case ISwSketchSegment seg:
                            res.Add(seg.Definition);
                            break;
                    }
                }

                return res.ToArray();
            });
        }
    }

    internal static class SwRegionExtension
    {
        internal static ISwTempPlanarSheetBody ToPlanarSheetBody(this ISwPlanarRegion region, ISwMemoryGeometryBuilder geomBuilder)
        {
            var planarSheet = geomBuilder.CreatePlanarSheet(region);
            var bodies = planarSheet.Bodies;

            if (bodies.Length == 1)
            {
                return (ISwTempPlanarSheetBody)bodies.First();
            }
            else 
            {
                throw new Exception("Region must contain only one planar sheet body");
            }
        }
    }
}
