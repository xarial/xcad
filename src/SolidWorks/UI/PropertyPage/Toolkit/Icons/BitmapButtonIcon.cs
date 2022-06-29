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
    internal class BitmapButtonIcon : IIcon
    {
        protected const int BORDER_SIZE = 3;

        internal IXImage Icon { get; }

        protected readonly int m_Width;
        protected readonly int m_Height;

        public Color TransparencyKey => Color.White;

        public bool IsPermanent => false;

        internal BitmapButtonIcon(IXImage icon, int width, int height)
        {
            Icon = icon;
            m_Width = width;
            m_Height = height;
        }
        
        public virtual IEnumerable<IIconSpec> GetIconSizes()
        {
            yield return new IconSpec(Icon, new Size(m_Width, m_Height), BORDER_SIZE);
            yield return new IconSpec(Icon, new Size(m_Width, m_Height), CreateMask, BORDER_SIZE);
        }

        protected void CreateMask(ref byte r, ref byte g, ref byte b, ref byte a) 
        {
            var mask = (byte)(255 - a);
            r = mask;
            g = mask;
            b = mask;
            a = 255;
        }
    }
}