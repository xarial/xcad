//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;
using Xarial.XCad.SolidWorks.Geometry.Extensions;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Geometry.Primitives
{
    public interface ISwTempExtrusion : IXExtrusion, ISwTempPrimitive
    {
        new ISwPlanarRegion[] Profiles { get; set; }
    }

    public interface ISwTempSolidExtrusion : ISwTempExtrusion 
    {
    }

    public interface ISwTempSurfaceExtrusion : ISwTempExtrusion, IXSheetExtrusion
    {
        new ISwCurve[] Profiles { get; set; }
    }

    internal abstract class SwTempExtrusion : SwTempPrimitive, ISwTempExtrusion
    {
        IXPlanarRegion[] IXExtrusion.Profiles
        {
            get => Profiles;
            set => Profiles = value?.Cast<ISwPlanarRegion>().ToArray();
        }

        public double Depth
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

        public Vector Direction
        {
            get => m_Creator.CachedProperties.Get<Vector>();
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

        public ISwPlanarRegion[] Profiles
        {
            get => m_Creator.CachedProperties.Get<ISwPlanarRegion[]>();
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

        internal SwTempExtrusion(SwTempBody[] bodies, ISwApplication app, bool isCreated)
            : base(bodies, app, isCreated)
        {
        }

        protected override ISwTempBody[] CreateBodies(CancellationToken cancellationToken) 
            => throw new NotImplementedException();
    }

    internal class SwTempSolidExtrusion : SwTempExtrusion, ISwTempSolidExtrusion
    {
        internal SwTempSolidExtrusion(SwTempBody[] bodies, ISwApplication app, bool isCreated)
            : base(bodies, app, isCreated)
        {
        }

        protected override ISwTempBody[] CreateBodies(CancellationToken cancellationToken)
        {
            var dir = m_MathUtils.CreateVector(Direction.ToArray()) as MathVector;

            var bodies = new List<ISwTempBody>();

            foreach (var profile in Profiles)
            {
                var length = Depth;

                if (length == 0)
                {
                    throw new Exception("Cannot create extrusion of 0 length");
                }

                var body = (IBody2)m_Modeler.CreateExtrudedBody((Body2)profile.PlanarSheetBody.Body, dir, length);

                if (body == null)
                {
                    throw new Exception("Failed to create extrusion");
                }

                bodies.Add(m_App.CreateObjectFromDispatch<SwTempBody>(body, null));
            }

            return bodies.ToArray();
        }
    }

    internal class SwTempSurfaceExtrusion : SwTempExtrusion, ISwTempSurfaceExtrusion
    {
        internal SwTempSurfaceExtrusion(SwTempBody[] bodies, ISwApplication app, bool isCreated)
            : base(bodies, app, isCreated)
        {
        }

        ISwCurve[] ISwTempSurfaceExtrusion.Profiles
        {
            get => m_Creator.CachedProperties.Get<ISwCurve[]>(nameof(Profiles) + "%Curve");
            set
            {
                if (IsCommitted)
                {
                    throw new CommitedSegmentReadOnlyParameterException();
                }
                else
                {
                    m_Creator.CachedProperties.Set(value, nameof(Profiles) + "%Curve");
                }
            }
        }

        IXCurve[] IXSheetExtrusion.Profiles
        {
            get => ((ISwTempSurfaceExtrusion)this).Profiles;
            set => ((ISwTempSurfaceExtrusion)this).Profiles = value?.Cast<ISwCurve>().ToArray();
        }

        protected override ISwTempBody[] CreateBodies(CancellationToken cancellationToken)
        {
            var bodies = new List<ISwTempBody>();

            var length = Depth;

            if (length > 0)
            {
                var curveProfiles = ((ISwTempSurfaceExtrusion)this).Profiles;

                if (curveProfiles != null)
                {
                    foreach (var curveProfile in curveProfiles)
                    {
                        bodies.Add(CreateExtrusionFromCurves(new ISwCurve[] { curveProfile }, Direction, length));
                    }
                }

                var sheetProfiles = Profiles;

                if (sheetProfiles != null)
                {
                    foreach (var sheetProfile in sheetProfiles)
                    {
                        foreach (var loop in new ISwLoop[] { sheetProfile.OuterLoop }.Union(sheetProfile.InnerLoops ?? new ISwLoop[0]))
                        {
                            bodies.Add(CreateExtrusionFromCurves(loop.IterateCurves().ToArray(), Direction, length));
                        }
                    }
                }

                if (bodies.Any())
                {
                    return bodies.ToArray();
                }
                else
                {
                    throw new Exception("No profiles specified");
                }
            }
            else
            {
                throw new Exception("Extrusion length must be more than 0");
            }
        }

        private ISwTempBody CreateExtrusionFromCurves(ISwCurve[] curves, Vector dir, double length)
        {
            if (curves.Any())
            {
                var axisDir = dir.ToArray();

                ISwTempBody extrBody = null;

                foreach (var curve in curves)
                {
                    foreach (var swCurve in curve.Curves)
                    {
                        var surf = (ISurface)m_Modeler.CreateExtrusionSurface(swCurve.ICopy(), axisDir);

                        if (surf != null)
                        {
                            Curve[] trimCurves;

                            var transform = TransformMatrix.CreateFromTranslation(Direction.Normalize().Scale(length));

                            swCurve.GetEndParams(out var startParam, out var endParam, out var isClosed, out _);

                            var baseTrimCurve = swCurve.ICopy();
                            var oppositeTrimCurve = swCurve.ICopy();
                            oppositeTrimCurve.ApplyTransform((MathTransform)m_MathUtils.ToMathTransform(transform));

                            if (isClosed)
                            {
                                trimCurves = new Curve[]
                                {
                                    baseTrimCurve,
                                    null,
                                    oppositeTrimCurve
                                };
                            }
                            else
                            {
                                var startCoord = (double[])swCurve.Evaluate2(startParam, 0);
                                var endCoord = (double[])swCurve.Evaluate2(endParam, 0);

                                var startPt = new Point(startCoord[0], startCoord[1], startCoord[2]);
                                var endPt = new Point(endCoord[0], endCoord[1], endCoord[2]);

                                trimCurves = new Curve[]
                                {
                                    baseTrimCurve,
                                    m_Modeler.CreateTrimmedLine(startPt, startPt.Move(dir, length)),
                                    oppositeTrimCurve,
                                    m_Modeler.CreateTrimmedLine(endPt.Move(dir, length), endPt)
                                };
                            }

                            IBody2 sheetBody;

                            if (m_App.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2017, 4))
                            {
                                sheetBody = surf.CreateTrimmedSheet5(trimCurves, true,
                                    ((SwMemoryGeometryBuilder)m_App.MemoryGeometryBuilder).TolProvider.Trimming) as Body2;
                            }
                            else
                            {
                                sheetBody = surf.CreateTrimmedSheet4(trimCurves, true) as Body2;
                            }

                            if (sheetBody != null)
                            {
                                var body = m_App.CreateObjectFromDispatch<SwTempBody>(sheetBody, null);

                                if (extrBody == null)
                                {
                                    extrBody = body;
                                }
                                else 
                                {
                                    //TODO: implement cheking the connection between bodies to ensure bodies can be joined in the correct order
                                    extrBody = extrBody.Add(body);
                                }
                            }
                            else
                            {
                                throw new Exception("Failed to create profile sheet body");
                            }
                        }
                        else
                        {
                            throw new Exception("Failed to create surface");
                        }
                    }
                }

                return extrBody;
            }
            else 
            {
                throw new Exception("No curves in the extrusion profile");
            }
        }
    }
}
