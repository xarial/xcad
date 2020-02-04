//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Drawing;

namespace Xarial.XCad.SolidWorks.Base
{
    /// <summary>
    /// Descriptor for the icon of a specific type
    /// </summary>
    internal class IconSizeInfo
    {
        /// <summary>
        /// Base name of the icon
        /// </summary>
        internal string Name { get; private set; }

        /// <summary>
        /// Original image of the icon
        /// </summary>
        internal Image SourceImage { get; private set; }

        /// <summary>
        /// Required size of the icon
        /// </summary>
        internal Size TargetSize { get; private set; }

        /// <summary>
        /// Icon size constructor with source image, target size and optional base name
        /// </summary>
        /// <param name="srcImage">Source image</param>
        /// <param name="targetSize">Target size of the image</param>
        /// <param name="baseName">Base name of the image</param>
        internal IconSizeInfo(Image srcImage, Size targetSize, string baseName = "")
        {
            SourceImage = srcImage;
            TargetSize = targetSize;

            Name = CreateFileName(baseName, targetSize);
        }

        /// <summary>
        /// Generates the file name for the icon
        /// </summary>
        /// <param name="baseName">Base name for the icon</param>
        /// <param name="targetSize">Required icon size</param>
        /// <returns>Suggested file name</returns>
        internal static string CreateFileName(string baseName, Size targetSize)
        {
            if (string.IsNullOrEmpty(baseName))
            {
                baseName = Guid.NewGuid().ToString();
            }

            return $"{baseName}_{targetSize.Width}x{targetSize.Height}.bmp";
        }
    }
}