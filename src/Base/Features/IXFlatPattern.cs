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
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Features
{
    /// <summary>
    /// Represents the flat pattern
    /// </summary>
    public interface IXFlatPattern : IXFeature
    {
        /// <summary>
        /// Gets or sets if this flat pattern feature is flattened
        /// </summary>
        bool IsFlattened { get; set; }

        /// <summary>
        /// Entity which is used as a fixed face
        /// </summary>
        IXEntity FixedEntity { get; }

        /// <summary>
        /// Pre-creates save-as operation
        /// </summary>
        /// <param name="filePath">Output file path</param>
        IFlatPatternSaveOperation PreCreateSaveAsOperation(string filePath);
    }
}
