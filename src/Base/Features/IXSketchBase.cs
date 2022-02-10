//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Sketch;

namespace Xarial.XCad.Features
{
    /// <summary>
    /// Represents the base sketch 2D or 3D
    /// </summary>
    public interface IXSketchBase : IXFeature
    {
        /// <summary>
        /// Identifies if this sketch is currently under editing
        /// </summary>
        bool IsEditing { get; set; }

        /// <summary>
        /// List of sketch entitites (segments and points)
        /// </summary>
        IXSketchEntityRepository Entities { get; }
    }
}