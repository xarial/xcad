//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.SwDocumentManager.Geometry
{
    public interface ISwDmSolidBody : ISwDmBody, IXSolidBody 
    {
    }

    internal class SwDmSolidBody : SwDmBody, ISwDmSolidBody
    {
        public double Volume => throw new NotSupportedException();
    }
}
