//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using System.Drawing;
using Xarial.XCad.SolidWorks.Base;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons
{
    internal class BitmapButtonIcon : IIcon
    {
        internal Image Icon { get; private set; }
        internal Image Mask { get; private set; }

        protected readonly int m_Width;
        protected readonly int m_Height;

        public Color TransparencyKey
        {
            get
            {
                return Color.White;
            }
        }

        internal BitmapButtonIcon(Image icon, int width, int height)
            : this(icon, CreateMask(icon))
        {
            m_Width = width;
            m_Height = height;
        }
        
        private BitmapButtonIcon(Image icon, Image mask)
        {
            Icon = icon;
            Mask = mask;
        }

        public virtual IEnumerable<IconSizeInfo> GetIconSizes()
        {
            yield return new IconSizeInfo(Icon, new Size(m_Width, m_Height));
            yield return new IconSizeInfo(Mask, new Size(m_Width, m_Height));
        }

        private static Image CreateMask(Image icon)
        {
            return IconsConverter.ReplaceColor(icon,
                new IconsConverter.ColorReplacerDelegate((ref byte r, ref byte g, ref byte b, ref byte a) =>
                {
                    var mask = (byte)(255 - a);
                    r = mask;
                    g = mask;
                    b = mask;
                    a = 255;
                }));
        }
    }
}