using System.Collections.Generic;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Documentation
{
    public class MacroFeatureSelectionParams
    {
        //selection parameter of any entity (e.g. face, edge, feature etc.)
        public IXSelObject AnyEntity { get; set; }

        //selection parameter of body
        public IXBody Body { get; set; }

        //selection parameter of array of faces
        public List<IXFace> Faces { get; set; }
    }
}
