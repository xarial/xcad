//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using System.Drawing;

namespace Xarial.XCad.Toolkit.Base
{
    /// <summary>
    /// Format of the image icon
    /// </summary>
    public enum IconImageFormat_e
    {
        /// <summary>
        /// .bmp
        /// </summary>
        Bmp,

        /// <summary>
        /// .png
        /// </summary>
        Png,

        /// <summary>
        /// .jpeg
        /// </summary>
        Jpeg
    }

    /// <summary>
    /// Represents the specific icon descriptor
    /// </summary>
    public interface IIcon
    {
        /// <summary>
        /// Indicates that this icon is permanent and should not be removed on dispose
        /// </summary>
        bool IsPermanent { get; }

        /// <summary>
        /// Transparency key to be applied to transparent color
        /// </summary>
        Color TransparencyKey { get; }

        /// <summary>
        /// List of required icon sizes
        /// </summary>
        /// <returns></returns>
        IIconSpec[] IconSizes { get; }

        /// <summary>
        /// Image format
        /// </summary>
        IconImageFormat_e Format { get; }
    }
}