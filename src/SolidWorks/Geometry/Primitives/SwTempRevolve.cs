//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Primitives
{
    public interface ISwTempRevolve : IXRevolve, ISwTempPrimitive
    {
        new ISwTempRegion[] Profiles { get; set; }
        new ISwLineCurve Axis { get; set; }
    }

    public interface ISwTempSolidRevolve : ISwTempRevolve 
    {
    }

    internal class SwTempSolidRevolve : SwTempPrimitive, ISwTempSolidRevolve
    {
        internal SwTempSolidRevolve(SwTempBody[] bodies, ISwApplication app, bool isCreated) 
            : base(bodies, app, isCreated)
        {
        }

        IXPlanarRegion[] IXRevolve.Profiles 
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

        protected override ISwTempBody[] CreateBodies(CancellationToken cancellationToken)
        {
            var bodies = new List<ISwTempBody>();

            foreach (var profile in Profiles) 
            {
                var sheetBody = profile.PlanarSheetBody;

                var profileLoop = sheetBody.Body.IGetFirstFace().IGetFirstLoop();

                var line = Axis.Geometry;

                var axisPt = line.StartPoint;
                var axisDir = line.EndPoint - line.StartPoint;

                var body = profileLoop.RevolvePlanarLoop(
                    axisPt.X, axisPt.Y, axisPt.Z,
                    axisDir.X, axisDir.Y, axisDir.Z, Angle) as object[];

                if (body == null || body.FirstOrDefault() == null)
                {
                    throw new Exception("Failed to create revolve body");
                }

                bodies.Add(m_App.CreateObjectFromDispatch<SwTempBody>(body.First(), null));
            }
            
            return bodies.ToArray();
        }
    }
}
