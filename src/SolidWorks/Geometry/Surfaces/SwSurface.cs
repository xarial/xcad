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

        internal SwSurface(ISurface surface) : base(surface)
        {
            Surface = surface;
        }
    }
}
