//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using System.Drawing;
using Xarial.XCad.SolidWorks.Base;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Base;
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.UI;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons
{
    internal class BitmapButtonIcon : IIcon
    {
        protected const int BORDER_SIZE = 3;

        internal IXImage Icon { get; }

        protected readonly int m_Width;
        protected readonly int m_Height;

        public Color TransparencyKey => Color.White;

        public bool IsPermanent => false;

        public IconImageFormat_e Format => IconImageFormat_e.Bmp;

        internal BitmapEffect_e Effect { get; }

        internal BitmapButtonIcon(IXImage icon, int width, int height, BitmapEffect_e effect = BitmapEffect_e.None)
        {
            Icon = icon;
            m_Width = width;
            m_Height = height;

            Effect = effect;

            IconSizes = new IIconSpec[]
            {
                new IconSpec(Icon, new Size(m_Width, m_Height), ApplyEffect, BORDER_SIZE),
                new IconSpec(Icon, new Size(m_Width, m_Height), CreateMask, BORDER_SIZE)
            };
        }

        public virtual IIconSpec[] IconSizes { get; }

        protected void ConvertPixelToGrayscale(ref byte r, ref byte g, ref byte b, ref byte a)
            => ColorUtils.ConvertPixelToGrayscale(ref r, ref g, ref b);

        protected void CreateMask(ref byte r, ref byte g, ref byte b, ref byte a) 
        {
            var mask = (byte)(255 - a);
            r = mask;
            g = mask;
            b = mask;
            a = 255;
        }

        protected void ApplyEffect(ref byte r, ref byte g, ref byte b, ref byte a)
        {
            if (Effect.HasFlag(BitmapEffect_e.Grayscale))
            {
                ColorUtils.ConvertPixelToGrayscale(ref r, ref g, ref b);
            }

            if (Effect.HasFlag(BitmapEffect_e.Transparent))
            {
                a = (byte)((double)a / 2);
            }
        }
    }
}