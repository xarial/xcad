//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwRegion : IXRegion
    {
        new ISwCurve[] Boundary { get; }
    }

    internal class SwRegion : ISwRegion 
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

        private readonly Lazy<ISwCurve[]> m_LazyBoundary;

        internal SwRegion(IXSegment[] boundary) 
        {
            Boundary = boundary;

            //TODO: check if segment is not curve - then create curve (e.g. from the edge)
            m_LazyBoundary = new Lazy<ISwCurve[]>(() => boundary.Cast<ISwCurve>().ToArray());
        }
    }
}
