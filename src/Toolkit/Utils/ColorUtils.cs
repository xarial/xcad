//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Xarial.XCad.Toolkit.Utils
{
    public static class ColorUtils
    {
        public static int ToColorRef(Color color)
        {
            return (color.R << 0) | (color.G << 8) | (color.B << 16);
        }

        public static Color FromColorRef(int colorRef) 
        {
            int r = colorRef & 0x000000FF;
            int g = (colorRef & 0x0000FF00) >> 8;
            int b = (colorRef & 0x00FF0000) >> 16;

            return Color.FromArgb(r, g, b);
        }
    }
}
