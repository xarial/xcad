//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Surfaces;

namespace Xarial.XCad.SolidWorks.Geometry.Surfaces
{
    public interface ISwSurface : IXSurface, ISwObject
    {
        ISurface Surface { get; }
    }

    internal class SwSurface : SwObject, ISwSurface
    {
        public ISurface Surface { get; }

        public override object Dispatch => Surface;

        internal SwSurface(ISurface surface) : base(surface)
        {
            Surface = surface;
        }
    }
}
