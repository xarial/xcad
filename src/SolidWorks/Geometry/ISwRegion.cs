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
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Sketch;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwRegion : IXRegion
    {
        /// <summary>
        /// Boundary of this region
        /// </summary>
        new ISwLoop OuterLoop { get; set; }

        /// <summary>
        /// Inner loops in the region
        /// </summary>
        new ISwLoop[] InnerLoops { get; set; }
    }

    public interface ISwPlanarRegion : ISwRegion, IXPlanarRegion
    {
        ISwTempPlanarSheetBody PlanarSheetBody { get; }
    }

    internal sealed class SwPlanarRegion : ISwPlanarRegion
    {
        IXLoop IXRegion.OuterLoop { get => OuterLoop; set => OuterLoop = (ISwLoop)value; }
        IXLoop[] IXRegion.InnerLoops { get => InnerLoops; set => InnerLoops = value.Cast<ISwLoop>().ToArray(); }

        public Plane Plane
        {
            get
            {
                Plane plane = null;

                var outerLoop = OuterLoop;

                var firstCurve = outerLoop.Curves.First() as SwCurve;

                if (firstCurve?.TryGetPlane(out plane) == true)
                {
                    return plane;
                }
                else
                {
                    //TODO: check if not colinear
                    //TODO: check if all on the same plane
                    //TODO: fix if a single curve

                    var refVec1 = outerLoop.Curves[0].EndPoint.Coordinate - outerLoop.Curves[0].StartPoint.Coordinate;
                    var refVec2 = outerLoop.Curves[1].EndPoint.Coordinate - outerLoop.Curves[1].StartPoint.Coordinate;
                    var normVec = refVec1.Cross(refVec2);

                    return new Plane(outerLoop.Curves[0].StartPoint.Coordinate, normVec, refVec1);
                }
            }
        }

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

                boundary.AddRange(IterateCurves(OuterLoop));

                const ICurve LOOP_SEPARATOR = null;

                foreach (var innerLoop in InnerLoops ?? new ISwLoop[0]) 
                {
                    boundary.Add(LOOP_SEPARATOR);
                    boundary.AddRange(IterateCurves(innerLoop));
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

        public ISwLoop OuterLoop 
        {
            get => m_Creator.CachedProperties.Get<ISwLoop>();
            set 
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else 
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        public ISwLoop[] InnerLoops
        {
            get => m_Creator.CachedProperties.Get<ISwLoop[]>();
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        public bool IsCommitted => m_Creator.IsCreated;

        private readonly IElementCreator<bool?> m_Creator;

        private readonly SwMemoryGeometryBuilder m_GeomBuilder;

        internal SwPlanarRegion(SwMemoryGeometryBuilder geomBuilder)
        {
            m_GeomBuilder = geomBuilder;
            m_Creator = new ElementCreator<bool?>(CreateRegion, null);
        }

        private bool? CreateRegion(CancellationToken cancellationToken) => true;

        private IEnumerable<ICurve> IterateCurves(IXLoop loop) 
        {
            foreach (var seg in loop.Segments) 
            {
                ISwCurve segCurve;

                switch (seg)
                {
                    case ISwCurve curve:
                        segCurve = curve;
                        break;

                    case ISwEdge edge:
                        segCurve = edge.Definition;
                        break;

                    case ISwSketchSegment skSeg:
                        segCurve = skSeg.Definition;
                        break;

                    default:
                        throw new NotSupportedException();
                }

                foreach (var subSegCurve in segCurve.Curves) 
                {
                    yield return subSegCurve;
                }
            }
        }

        public void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);
    }
}
