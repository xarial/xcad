//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Surfaces;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Surfaces;

namespace Xarial.XCad.SolidWorks.Geometry.Surfaces
{
    public interface ISwRevolvedSurface : ISwSurface, IXRevolvedSurface
    {
    }

    internal class SwRevolvedSurface : SwSurface, ISwRevolvedSurface
    {
        public SwRevolvedSurface(ISurface surface, SwDocument doc, SwApplication app) : base(surface, doc, app)
        {
        }
    }
}
