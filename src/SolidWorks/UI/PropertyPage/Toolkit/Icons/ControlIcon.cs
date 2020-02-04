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
    internal class ControlIcon : IIcon
    {
        internal Image Icon { get; private set; }
        internal Image Mask { get; private set; }

        public Color TransparencyKey
        {
            get
            {
                return Color.White;
            }
        }

        private readonly Size m_Size;

        internal ControlIcon(Image icon)
            : this(icon, CreateMask(icon))
        {
        }

        internal ControlIcon(Image icon, Size size)
            : this(icon, CreateMask(icon), size)
        {
        }

        internal ControlIcon(Image icon, Image mask)
            : this(icon, mask, new Size(24, 24))
        {
        }

        internal ControlIcon(Image icon, Image mask, Size size)
        {
            Icon = icon;
            Mask = mask;
            m_Size = size;
        }

        public IEnumerable<IconSizeInfo> GetIconSizes()
        {
            yield return new IconSizeInfo(Icon, m_Size);
            yield return new IconSizeInfo(Mask, m_Size);
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