using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Services;

namespace Xarial.XCad.SolidWorks.Geometry.Curves
{
    public class SwCurve : IXCurve
    {
        public ICurve Curve => m_Creator.Element;

        public IXPoint StartPoint => GetPoint(true);
        public IXPoint EndPoint => GetPoint(false);
        
        public bool IsCommitted => m_Creator.IsCreated;

        protected readonly ElementCreator<ICurve> m_Creator;

        protected readonly IModeler m_Modeler;

        internal SwCurve(IModeler modeler, ICurve curve, bool isCreated) 
        {
            m_Modeler = modeler;
            m_Creator = new ElementCreator<ICurve>(Create, curve, isCreated);
        }

        public void Commit() => m_Creator.Create();

        protected virtual ICurve Create() 
        {
            throw new NotSupportedException();
        }

        private IXPoint GetPoint(bool isStart)
        {
            if (Curve.IsTrimmedCurve())
            {
                if (Curve.GetEndParams(out double start, out double end, out _, out _))
                {
                    var pt = Curve.Evaluate2(isStart ? start : end, 1) as double[];
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
    }
}
