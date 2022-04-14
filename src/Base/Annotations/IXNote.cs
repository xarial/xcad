//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Annotations
{
    /// <summary>
    /// Represents the note annotation
    /// </summary>
    public interface IXNote : IXAnnotation
    {
        /// <summary>
        /// Boundary of this note
        /// </summary>
        Box3D Box { get; }

        /// <summary>
        /// Text of the note
        /// </summary>
        string Text { get; set; }
    }
}
