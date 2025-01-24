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
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Annotations
{
    /// <summary>
    /// Represents the section line annotation of <see cref="Documents.IXSectionDrawingView"/>
    /// </summary>
    public interface IXSectionLine : IXAnnotation
    {
        /// <summary>
        /// Geometry of the line
        /// </summary>
        Line Definition { get; set; }
    }
}
