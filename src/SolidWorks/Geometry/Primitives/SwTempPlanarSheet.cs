//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Primitives
{
    public interface ISwTempPlanarSheet : IXPlanarSheet, ISwTempPrimitive
    {
        new ISwTempPlanarSheetBody[] Bodies { get; }
        new ISwCurve[] Boundary { get; set; }
    }

    internal class SwTempPlanarSheet : SwTempPrimitive, ISwTempPlanarSheet
    {
        IXPlanarSheetBody[] IXPlanarSheet.Bodies => Bodies;

        IXSegment[] IXPlanarSheet.Boundary
        {
            get => Boundary;
            set => Boundary = value.Cast<SwCurve>().ToArray();
        }
        
        internal SwTempPlanarSheet(IMathUtility mathUtils, IModeler modeler, SwTempBody[] bodies, bool isCreated)
            : base(mathUtils, modeler, bodies, isCreated)
        {
        }

        public ISwCurve[] Boundary
        {
            get => m_Creator.CachedProperties.Get<SwCurve[]>();
            set
            {
                if (IsCommitted)
                {
                    throw new CommitedSegmentReadOnlyParameterException();
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public Plane Plane 
        {
            get 
            {
                Plane plane = null;

                if (Boundary.FirstOrDefault()?.TryGetPlane(out plane) == true)
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

        new public ISwTempPlanarSheetBody[] Bodies => base.Bodies.Cast<ISwTempPlanarSheetBody>().ToArray();

        protected override ISwTempBody[] CreateBodies(CancellationToken cancellationToken)
        {
            var plane = Plane;

            var planarSurf = m_Modeler.CreatePlanarSurface2(
                    plane.Point.ToArray(), plane.Normal.ToArray(), plane.Direction.ToArray()) as ISurface;

            if (planarSurf == null)
            {
                throw new Exception("Failed to create plane");
            }

            var boundary = new List<ICurve>();

            for (int i = 0; i < Boundary.Length; i++)
            {
                boundary.AddRange(Boundary[i].Curves);

                if (i != Boundary.Length - 1)
                {
                    boundary.Add(null);
                }
            }

            var sheetBody = planarSurf.CreateTrimmedSheet4(boundary.ToArray(), true) as Body2;

            if (sheetBody == null)
            {
                throw new Exception("Failed to create profile sheet body");
            }

            return new ISwTempBody[] { SwSelObject.FromDispatch<SwTempBody>(sheetBody) };
        }
    }
}
