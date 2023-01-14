//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Geometry.Curves
{
    public interface ISwPoint : ISwObject, IXPoint
    {
    }

    internal class SwPoint : SwObject, ISwPoint
    {
        internal SwPoint(object disp, SwDocument doc, SwApplication app) : base(disp, doc, app)
        {
        }

        public Point Coordinate { get; set; }

        public bool IsCommitted => true;

        public void Commit(CancellationToken cancellationToken)
        {
        }
    }

    internal class SwMathPoint : SwObject, ISwPoint
    {
        internal IMathPoint MathPoint { get; }

        internal SwMathPoint(IMathPoint mathPt, SwDocument doc, SwApplication app) : base(mathPt, doc, app)
        {
            MathPoint = mathPt;
        }

        public bool IsCommitted => true;

        public Point Coordinate 
        {
            get => new Point((double[])MathPoint.ArrayData);
            set => MathPoint.ArrayData = value.ToArray();
        }

        public void Commit(CancellationToken cancellationToken)
        {
        }
    }
}
