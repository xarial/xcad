//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Geometry.Curves
{
    public interface ISwCurve : IXCurve, ISwObject
    {
        ICurve[] Curves { get; }
    }

    internal class SwCurve : SwObject, ISwCurve
    {
        public ICurve[] Curves => m_Creator.Element;

        public IXPoint StartPoint => GetPoint(true);
        public IXPoint EndPoint => GetPoint(false);
        
        public bool IsCommitted => m_Creator.IsCreated;

        public double Length
        {
            get
            {
                if (Curves != null)
                {
                    return Curves.Sum(c =>
                    {
                        if (c.IsTrimmedCurve())
                        {
                            c.GetEndParams(out double start, out double end, out bool _, out bool _);

                            var length = c.GetLength3(start, end);
                            return length;
                        }
                        else
                        {
                            throw new Exception("Only trimmed curves are supported");
                        }
                    });
                }
                else 
                {
                    return double.NaN;
                }
            }
        }

        public override object Dispatch => Curves;

        protected readonly IElementCreator<ICurve[]> m_Creator;

        protected readonly IModeler m_Modeler;
        private readonly IMathUtility m_MathUtils;

        internal SwCurve(ICurve curve, SwDocument doc, SwApplication app, bool isCreated) 
            : this(new ICurve[] { curve }, doc, app, isCreated)
        { 
        }

        internal SwCurve(ICurve[] curves, SwDocument doc, SwApplication app, bool isCreated) : base(curves, doc, app)
        {
            m_Modeler = app.Sw.IGetModeler();
            m_MathUtils = app.Sw.IGetMathUtility();
            m_Creator = new ElementCreator<ICurve[]>(Create, curves, isCreated);
        }

        public void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        protected virtual ICurve[] Create(CancellationToken cancellationToken) 
        {
            throw new NotSupportedException();
        }

        protected virtual IXPoint GetPoint(bool isStart)
        {
            var curve = isStart ? Curves.First() : Curves.Last();

            if (curve.IsTrimmedCurve())
            {
                if (curve.GetEndParams(out double start, out double end, out _, out _))
                {
                    var pt = curve.Evaluate2(isStart ? start : end, 1) as double[];

                    return new SwPoint(null, OwnerDocument, OwnerApplication)
                    {
                        Coordinate = new Point(pt[0], pt[1], pt[2])
                    };
                }
                else
                {
                    throw new Exception("Failed to get end parameters of curve");
                }
            }
            else
            {
                throw new NotSupportedException("Only trimmed curves are supported");
            }
        }

        internal virtual bool TryGetPlane(out Plane plane)
        {
            plane = null;
            return false;
        }

        public Point FindClosestPoint(Point point)
        {
            Point resPt = null;

            foreach (var curve in Curves) 
            {
                var thisPt = new Point(((double[])curve.GetClosestPointOn(point.X, point.Y, point.Z)).Take(3).ToArray());

                if (resPt != null)
                {
                    if ((thisPt - point).GetLength() < (resPt - point).GetLength())
                    {
                        resPt = thisPt;
                    }
                }
            }

            return resPt;
        }

        public double CalculateUParameter(Point point)
        {
            if (Curves.Length == 1)
            {
                return Curves.First().ReverseEvaluate(point.X, point.Y, point.Z);
            }
            else 
            {
                throw new Exception("Only single curve is supported");
            }
        }

        public Point CalculateLocation(double uParam, out Vector tangent)
        {
            if (Curves.Length == 1)
            {
                var eval = (double[])Curves.First().Evaluate2(uParam, 1);

                tangent = new Vector(eval[3], eval[4], eval[5]);

                return new Point(eval[0], eval[1], eval[2]);
            }
            else
            {
                throw new Exception("Only single curve is supported");
            }
        }

        public double CalculateLength(double startParamU, double endParamU)
        {
            if (Curves.Length == 1)
            {
                return Curves.First().GetLength3(startParamU, endParamU);
            }
            else
            {
                throw new Exception("Only single curve is supported");
            }
        }

        public void GetUBoundary(out double uMin, out double uMax)
        {
            if (Curves.Length == 1)
            {
                if (!Curves.First().GetEndParams(out uMin, out uMax, out _, out _)) 
                {
                    throw new Exception("Failed to read end parameters of the curve");
                }
            }
            else
            {
                throw new Exception("Only single curve is supported");
            }
        }

        public void Transform(TransformMatrix transform)
        {
            foreach (var curve in Curves) 
            {
                curve.ApplyTransform((MathTransform)TransformConverter.ToMathTransform(m_MathUtils, transform));
            }
        }
    }
}
