//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Documents.Enums
{
    /// <summary>
    /// Direction of the view projection of <see cref="IXProjectedDrawingView"/>
    /// </summary>
    public enum ProjectedViewDirection_e
    {
        /// <summary>
        /// Left
        /// </summary>
        Left,

        /// <summary>
        /// Top
        /// </summary>
        Top,

        /// <summary>
        /// Right
        /// </summary>
        Right,

        /// <summary>
        /// Bottom
        /// </summary>
        Bottom,

        /// <summary>
        /// Isometric Top Left
        /// </summary>
        IsoTopLeft,

        /// <summary>
        /// Isometric Top Right
        /// </summary>
        IsoTopRight,

        /// <summary>
        /// Isometric Bottom Left
        /// </summary>
        IsoBottomLeft,

        /// <summary>
        /// Isolmetric Bottom Right
        /// </summary>
        IsoBottomRight
    }
}
