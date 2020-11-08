//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Services;

namespace Xarial.XCad.SolidWorks.Geometry.Curves
{
    public interface ISwCurve : IXCurve, ISwObject
    {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        bool TryGetPlane(out Plane plane);

        ICurve[] Curves { get; }
    }

    internal class SwCurve : SwObject, ISwCurve
    {
        public ICurve[] Curves => m_Creator.Element;

        public IXPoint StartPoint => GetPoint(true);
        public IXPoint EndPoint => GetPoint(false);
        
        public bool IsCommitted => m_Creator.IsCreated;

        protected readonly ElementCreator<ICurve[]> m_Creator;

        protected readonly IModeler m_Modeler;

        internal SwCurve(IModeler modeler, ICurve curve, bool isCreated) 
            : this(modeler, new ICurve[] { curve }, isCreated)
        { 
        }

        internal SwCurve(IModeler modeler, ICurve[] curves, bool isCreated) : base(curves)
        {
            m_Modeler = modeler;
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
                    return new SwPoint()
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

        public virtual bool TryGetPlane(out Plane plane)
        {
            plane = null;
            return false;
        }
    }
}
