using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Primitives
{
    public interface ISwTempRevolve : IXRevolve, ISwTempPrimitive
    {
        new ISwTempRegion Profile { get; set; }
        new ISwLineCurve Axis { get; set; }
    }

    internal class SwTempRevolve : SwTempPrimitive, ISwTempRevolve
    {
        internal SwTempRevolve(IMathUtility mathUtils, IModeler modeler, SwTempBody body, bool isCreated) 
            : base(mathUtils, modeler, body, isCreated)
        {
        }

        IXRegion IXRevolve.Profile 
        {
            get => Profile;
            set => Profile = (ISwTempRegion)value; 
        }

        IXLine IXRevolve.Axis
        {
            get => Axis;
            set => Axis = (ISwLineCurve)value;
        }
                
        public double Angle
        {
            get => m_Creator.CachedProperties.Get<double>();
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

        public ISwTempRegion Profile
        {
            get => m_Creator.CachedProperties.Get<ISwTempRegion>();
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

        public ISwLineCurve Axis
        {
            get => m_Creator.CachedProperties.Get<SwLineCurve>();
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

        protected override SwTempBody CreateBody()
        {
            var plane = Profile.Plane;

            //var planarSurf = m_Modeler.CreatePlanarSurface2(
            //    plane.Point.ToArray(), plane.Normal.ToArray(), plane.Direction.ToArray()) as ISurface;

            //if (planarSurf == null) 
            //{
            //    throw new Exception("Failed to create plane");
            //}

            //var sheetBody = planarSurf.CreateTrimmedSheet4(Profile.Boundary.Curves, true) as Body2;

            //if (sheetBody == null) 
            //{
            //    throw new Exception("Failed to create profile sheet body");
            //}

            var sheetBody = Profile.Body.Body;

            var profileLoop = sheetBody.IGetFirstFace().IGetFirstLoop();

            var axisPt = Axis.StartCoordinate;
            var axisDir = Axis.EndCoordinate - Axis.StartCoordinate;

            var body = profileLoop.RevolvePlanarLoop(
                axisPt.X, axisPt.Y, axisPt.Z, 
                axisDir.X, axisDir.Y, axisDir.Z, Angle) as object[];

            if (body == null || body.FirstOrDefault() == null) 
            {
                throw new Exception("Failed to create revolve body");
            }

            return SwSelObject.FromDispatch<SwTempBody>(body.First());
        }
    }
}
