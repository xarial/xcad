//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Drawing;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.Base
{
    /// <summary>
    /// Custom handler for the image replace function
    /// </summary>
    /// <param name="r">Red component of pixel</param>
    /// <param name="g">Green component of pixel</param>
    /// <param name="b">Blue component of pixel</param>
    /// <param name="a">Alpha component of pixel</param>
    public delegate void ColorMaskDelegate(ref byte r, ref byte g, ref byte b, ref byte a);

    /// <summary>
    /// Descriptor for the icon of a specific type
    /// </summary>
    public interface IIconSpec 
    {
        /// <summary>
        /// Base name of the icon
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Original image of the icon
        /// </summary>
        IXImage SourceImage { get; }

        /// <summary>
        /// Required size of the icon
        /// </summary>
        Size TargetSize { get; }

        /// <summary>
        /// Handler for the mask
        /// </summary>
        ColorMaskDelegate Mask { get; }

        /// <summary>
        /// Image offset
        /// </summary>
        int Offset { get; }
    }

    /// <inheritdoc/>
    internal class IconSpec : IIconSpec
    {
        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public IXImage SourceImage { get; }

        /// <inheritdoc/>
        public Size TargetSize { get; }

        /// <inheritdoc/>
        public ColorMaskDelegate Mask { get; }

        /// <inheritdoc/>
        public int Offset { get; }

        /// <summary>
        /// Icon size constructor with source image, target size and optional base name
        /// </summary>
        /// <param name="srcImage">Source image</param>
        /// <param name="targetSize">Target size of the image</param>
        /// <param name="baseName">Base name of the image</param>
        internal IconSpec(IXImage srcImage, Size targetSize, int offset = 0, string baseName = "")
        {
            SourceImage = srcImage;
            TargetSize = targetSize;
            Offset = offset;

            Name = CreateFileName(baseName, targetSize);
        }

        internal IconSpec(IXImage srcImage, Size targetSize, ColorMaskDelegate mask, int offset = 0, string baseName = "")
            : this(srcImage, targetSize, offset, baseName)
        {
            Mask = mask;
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