//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Sketch;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwRegion : IXRegion
    {
        new ISwLoop[] Boundary { get; }
    }

    public interface ISwPlanarRegion : ISwRegion, IXPlanarRegion
    {
        ISwTempPlanarSheetBody PlanarSheetBody { get; }
    }

    internal sealed class SwPlanarRegion : ISwPlanarRegion
    {
        IXLoop[] IXRegion.Boundary => Boundary;

        public Plane Plane
        {
            get
            {
                Plane plane = null;

                var firstLoop = Boundary.First();

                var firstCurve = firstLoop.Curves.First() as SwCurve;

                if (firstCurve?.TryGetPlane(out plane) == true)
                {
                    return plane;
                }
                else
                {
                    //TODO: check if not colinear
                    //TODO: check if all on the same plane
                    //TODO: fix if a single curve

                    var refVec1 = firstLoop.Curves[0].EndPoint.Coordinate - firstLoop.Curves[0].StartPoint.Coordinate;
                    var refVec2 = firstLoop.Curves[1].EndPoint.Coordinate - firstLoop.Curves[1].StartPoint.Coordinate;
                    var normVec = refVec1.Cross(refVec2);

                    return new Plane(firstLoop.Curves[0].StartPoint.Coordinate, normVec, refVec1);
                }
            }
        }

        public ISwLoop[] Boundary => m_LazyBoundary.Value;

        public ISwTempPlanarSheetBody PlanarSheetBody
        {
            get
            {
                var plane = Plane;

                var planarSurf = m_GeomBuilder.Modeler.CreatePlanarSurface2(
                        plane.Point.ToArray(), plane.Normal.ToArray(), plane.Direction.ToArray()) as ISurface;

                if (planarSurf == null)
                {
                    throw new Exception("Failed to create plane");
                }

                var boundary = new List<ICurve>();

                var boundaryLoops = Boundary;

                const ICurve LOOP_SEPARATOR = null;

                for (int i = 0; i < boundaryLoops.Length; i++)
                {
                    boundary.AddRange(boundaryLoops[i].Curves.SelectMany(c => c.Curves).ToArray());

                    if (i != boundaryLoops.Length - 1)
                    {
                        boundary.Add(LOOP_SEPARATOR);
                    }
                }

                IBody2 sheetBody;

                if (m_GeomBuilder.Application.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2017, 4))
                {
                    sheetBody = planarSurf.CreateTrimmedSheet5(boundary.ToArray(), true, m_GeomBuilder.TolProvider.Trimming) as Body2;
                }
                else
                {
                    sheetBody = planarSurf.CreateTrimmedSheet4(boundary.ToArray(), true) as Body2;
                }

                if (sheetBody == null)
                {
                    throw new Exception("Failed to create profile sheet body");
                }

                return m_GeomBuilder.Application.CreateObjectFromDispatch<ISwTempPlanarSheetBody>(sheetBody, null);
            }
        }

        private readonly Lazy<ISwLoop[]> m_LazyBoundary;

        private readonly SwMemoryGeometryBuilder m_GeomBuilder;

        internal SwPlanarRegion(IXSegment[] boundary, SwMemoryGeometryBuilder geomBuilder)
        {
            m_GeomBuilder = geomBuilder;

            m_LazyBoundary = new Lazy<ISwLoop[]>(() =>
            {
                var loop = (ISwLoop)geomBuilder.WireBuilder.PreCreateLoop();

                var curves = new List<ISwCurve>();

                foreach (var bound in boundary) 
                {
                    switch (bound) 
                    {
                        case ISwCurve curve:
                            curves.Add(curve);
                            break;

                        case ISwEdge edge:
                            curves.Add(edge.Definition);
                            break;

                        case ISwSketchSegment seg:
                            curves.Add(seg.Definition);
                            break;
                    }
                }

                loop.Curves = curves.ToArray();
                loop.Commit();
                return new ISwLoop[] { loop };
            });
        }
    }
}
