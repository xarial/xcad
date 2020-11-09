//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.SolidWorks.Geometry.Curves
{
    public interface ISwPoint : IXPoint 
    {
    }

    internal class SwPoint : ISwPoint
    {
        public Point Coordinate { get; set; }

        public bool IsCommitted => true;

        public void Commit(CancellationToken cancellationToken)
        {
        }
    }
}
