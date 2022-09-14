using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.UI;

namespace Xarial.XCad.Sketch
{
    /// <summary>
    /// Represents sketch picture
    /// </summary>
    public interface IXSketchPicture : IXSketchEntity, IXFeature
    {
        /// <summary>
        /// Image of this picture
        /// </summary>
        IXImage Image { get; set; }

        /// <summary>
        /// Boundary of this picture
        /// </summary>
        Rect2D Boundary { get; set; }
    }
}
