//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xarial.XCad.Toolkit.Base;

namespace Xarial.XCad.Toolkit.Services
{
    /// <summary>
    /// Icon container
    /// </summary>
    public interface IIcon : IDisposable
    {
        /// <summary>
        /// Image of the icon
        /// </summary>
        Image Image { get; }

        /// <summary>
        /// Local file path of the icon
        /// </summary>
        string FilePath { get; }
    }

    /// <summary>
    /// Container for icons
    /// </summary>
    /// <remarks>This container is used to automate disposing of temp icons</remarks>
    public interface IIconCollection : IDisposable, IEnumerable<IIcon>
    {
        /// <summary>
        /// Gets image by index
        /// </summary>
        /// <param name="index">Image index</param>
        /// <returns></returns>
        IIcon this[int index] { get; }
    }

    /// <summary>
    /// Creates images from icons
    /// </summary>
    public interface IIconsCreator : IDisposable
    {
        /// <summary>
        /// Creates image from the icon in all sizes
        /// </summary>
        /// <param name="icon">Icon</param>
        /// <param name="folder">Custom folder, if empty - default folder is used</param>
        /// <returns>Paths to icons of all sizes</returns>
        IIconCollection ConvertIcon(Base.IIconDescriptor icon, string folder = "");

        /// <summary>
        /// Creates group of images from the input icons
        /// </summary>
        /// <param name="icons">Icons to group</param>
        /// <param name="folder">Custom folder, if empty - default folder is used</param>
        ///<inheritdoc/>
        IIconCollection ConvertIconsGroup(Base.IIconDescriptor[] icons, string folder = "");
    }
}
