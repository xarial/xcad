using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Primitives
{
    public interface ISwTempRevolve : IXRevolve, ISwTempPrimitive
    {
        new ISwTempRegion[] Profiles { get; set; }
        new ISwLineCurve Axis { get; set; }
    }

    internal class SwTempRevolve : SwTempPrimitive, ISwTempRevolve
    {
        internal SwTempRevolve(IMathUtility mathUtils, IModeler modeler, SwTempBody[] bodies, bool isCreated) 
            : base(mathUtils, modeler, bodies, isCreated)
        {
        }

        IXRegion[] IXRevolve.Profiles 
        {
            get => Profiles;
            set => Profiles = value.Cast<ISwTempRegion>().ToArray();
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

        public ISwTempRegion[] Profiles
        {
            get => m_Creator.CachedProperties.Get<ISwTempRegion[]>();
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

        protected override SwTempBody[] CreateBodies()
        {
            var bodies = new List<SwTempBody>();

            foreach (var profile in Profiles) 
            {
                var sheetBody = profile.Body;

                var profileLoop = sheetBody.Body.IGetFirstFace().IGetFirstLoop();

                var axisPt = Axis.StartCoordinate;
                var axisDir = Axis.EndCoordinate - Axis.StartCoordinate;

                var body = profileLoop.RevolvePlanarLoop(
                    axisPt.X, axisPt.Y, axisPt.Z,
                    axisDir.X, axisDir.Y, axisDir.Z, Angle) as object[];

                if (body == null || body.FirstOrDefault() == null)
                {
                    throw new Exception("Failed to create revolve body");
                }

                bodies.Add(SwSelObject.FromDispatch<SwTempBody>(body.First()));
            }
            
            return bodies.ToArray();
        }
    }
}
