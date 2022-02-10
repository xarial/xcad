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
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons
{
    internal class ControlIcon : IIcon
    {
        internal IXImage Icon { get; private set; }
        internal IXImage Mask { get; private set; }

        public Color TransparencyKey
        {
            get
            {
                return Color.White;
            }
        }

        private readonly Size m_Size;
        
        internal ControlIcon(IXImage icon)
            : this(icon, new Size(24, 24))
        {
        }

        internal ControlIcon(IXImage icon, Size size)
        {
            Icon = icon;
            m_Size = size;
        }

        public IEnumerable<IIconSpec> GetIconSizes()
        {
            yield return new IconSpec(Icon, m_Size);
            yield return new IconSpec(Icon, m_Size, CreateMask);
        }
        
        private void CreateMask(ref byte r, ref byte g, ref byte b, ref byte a)
        {
            var mask = (byte)(255 - a);
            r = mask;
            g = mask;
            b = mask;
            a = 255;
        }
    }
}