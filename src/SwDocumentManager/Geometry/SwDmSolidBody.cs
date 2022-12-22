//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
using Xarial.XCad.SwDocumentManager.Documents;

namespace Xarial.XCad.SwDocumentManager.Geometry
{
    public interface ISwDmSolidBody : ISwDmBody, IXSolidBody 
    {
    }

    internal class SwDmSolidBody : SwDmBody, ISwDmSolidBody
    {
        public SwDmSolidBody(SwDmPart part) : base(part)
        {
        }

        public double Volume => throw new NotSupportedException();
    }
}
