//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
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
        /// List of sketch entitites (segments and points)
        /// </summary>
        IXSketchEntityRepository Entities { get; }

        /// <summary>
        /// Manages the blank state (hidden/visible) of the sketch
        /// </summary>
        bool IsBlank { get; set; }
    }
}