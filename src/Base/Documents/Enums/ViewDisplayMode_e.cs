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
    /// Represents the display mode of the <see cref="IXDrawingView"/> and <see cref="IXModelView"/>
    /// </summary>
    public enum ViewDisplayMode_e
    {
        /// <summary>
        /// Wireframe
        /// </summary>
        Wireframe,

        /// <summary>
        /// Hidden Lines Visible
        /// </summary>
        HiddenLinesVisible,

        /// <summary>
        /// Hidden Lines Removed
        /// </summary>
        HiddenLinesRemoved,

        /// <summary>
        /// Shaded With Edges
        /// </summary>
        ShadedWithEdges,

        /// <summary>
        /// Shaded
        /// </summary>
        Shaded
    }
}
