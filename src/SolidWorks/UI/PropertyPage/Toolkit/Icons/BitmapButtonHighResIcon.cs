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
using Xarial.XCad.SolidWorks.Base;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons
{
    internal class BitmapButtonHighResIcon : BitmapButtonIcon
    {
        internal BitmapButtonHighResIcon(Image icon, int width, int height)
            : base(icon, width, height)
        {
        }

        public override IEnumerable<IconSizeInfo> GetIconSizes()
        {
            yield return new IconSizeInfo(Icon, new Size(AdjustSize(m_Width, 20), AdjustSize(m_Height, 20)));
            yield return new IconSizeInfo(Icon, new Size(AdjustSize(m_Width, 32), AdjustSize(m_Height, 32)));
            yield return new IconSizeInfo(Icon, new Size(AdjustSize(m_Width, 40), AdjustSize(m_Height, 40)));
            yield return new IconSizeInfo(Icon, new Size(AdjustSize(m_Width, 64), AdjustSize(m_Height, 64)));
            yield return new IconSizeInfo(Icon, new Size(AdjustSize(m_Width, 96), AdjustSize(m_Height, 96)));
            yield return new IconSizeInfo(Icon, new Size(AdjustSize(m_Width, 128), AdjustSize(m_Height, 128)));

            yield return new IconSizeInfo(Mask, new Size(AdjustSize(m_Width, 20), AdjustSize(m_Height, 20)));
            yield return new IconSizeInfo(Mask, new Size(AdjustSize(m_Width, 32), AdjustSize(m_Height, 32)));
            yield return new IconSizeInfo(Mask, new Size(AdjustSize(m_Width, 40), AdjustSize(m_Height, 40)));
            yield return new IconSizeInfo(Mask, new Size(AdjustSize(m_Width, 64), AdjustSize(m_Height, 64)));
            yield return new IconSizeInfo(Mask, new Size(AdjustSize(m_Width, 96), AdjustSize(m_Height, 96)));
            yield return new IconSizeInfo(Mask, new Size(AdjustSize(m_Width, 128), AdjustSize(m_Height, 128)));
        }

        /// <summary>
        /// Adjusting the size of the icons to match older version of SOLIDWORKS, so if add-in is updated from 2016 - the size of buttons remains the same
        /// </summary>
        private int AdjustSize(int baseSize, int actualSize)
        {
            const double DEFAULT_SIZE = 20; //default size of the button in newer versions of SOLIDWORKS

            var scale = baseSize / DEFAULT_SIZE;

            return (int)(scale * actualSize);
        }
    }
}
