//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.SolidWorks.Base;

namespace Xarial.XCad.SolidWorks.Services
{
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
        IImagesCollection ConvertIcon(IIcon icon, string folder = "");

        /// <summary>
        /// Creates group of images from the input icons
        /// </summary>
        /// <param name="icons">Icons to group</param>
        ///<inheritdoc/>
        IImagesCollection ConvertIconsGroup(IIcon[] icons, string folder = "");
    }
}
