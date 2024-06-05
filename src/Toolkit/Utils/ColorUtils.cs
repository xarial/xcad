﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Xarial.XCad.Toolkit.Utils
{
    /// <summary>
    /// Utility to convert between the .NET Color and Win32 color
    /// </summary>
    public static class ColorUtils
    {
        /// <summary>
        /// Creates a Win32 color
        /// </summary>
        /// <param name="color">Input color</param>
        /// <returns>Wind32 color</returns>
        public static int ToColorRef(Color color)
            => (color.R << 0) | (color.G << 8) | (color.B << 16);

        /// <summary>
        /// Converts Win32 color to .NET color
        /// </summary>
        /// <param name="colorRef">Input color</param>
        /// <returns>Converted color</returns>
        public static Color FromColorRef(int colorRef) 
        {
            int r = colorRef & 0x000000FF;
            int g = (colorRef & 0x0000FF00) >> 8;
            int b = (colorRef & 0x00FF0000) >> 16;

            return Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// Converts the pix to grayscale
        /// </summary>
        /// <param name="r">Red component of the pixel</param>
        /// <param name="g">Green component of the pixex</param>
        /// <param name="b">Blue component of the pixel</param>
        public static void ConvertPixelToGrayscale(ref byte r, ref byte g, ref byte b)
        {
            var pixel = (byte)((r + g + b) / 3);

            r = pixel;
            g = pixel;
            b = pixel;
        }
    }
}
