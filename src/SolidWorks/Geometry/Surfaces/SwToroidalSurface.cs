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
    public interface ISwToroidalSurface : ISwSurface, IXToroidalSurface
    {
    }

    internal class SwToroidalSurface : SwSurface, ISwToroidalSurface
    {
        public SwToroidalSurface(ISurface surface, ISwDocument doc, ISwApplication app) : base(surface, doc, app)
        {
        }
    }
}
