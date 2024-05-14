using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Enums
{
    /// <summary>
    /// Type of the face sheel
    /// </summary>
    public enum FaceShellType_e
    {
        /// <summary>
        /// Face of the <see cref="IXSheetBody"/>
        /// </summary>
        Open,

        /// <summary>
        /// Cavity face
        /// </summary>
        Internal,

        /// <summary>
        /// External face
        /// </summary>
        External
    }
}
