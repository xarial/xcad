//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xarial.XCad.SolidWorks.Base;
using Xarial.XCad.Toolkit.Base;
using Xarial.XCad.UI;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons
{
    internal class BitmapButtonHighResIcon : BitmapButtonIcon
    {
        internal BitmapButtonHighResIcon(IXImage icon, int width, int height, BitmapEffect_e effect = BitmapEffect_e.None)
            : base(icon, width, height, effect)
        {
            IconSizes = new IIconSpec[]
            {
                new IconSpec(Icon, new Size(AdjustSize(m_Width, 20), AdjustSize(m_Height, 20)), ApplyEffect, BORDER_SIZE),
                new IconSpec(Icon, new Size(AdjustSize(m_Width, 32), AdjustSize(m_Height, 32)), ApplyEffect, BORDER_SIZE),
                new IconSpec(Icon, new Size(AdjustSize(m_Width, 40), AdjustSize(m_Height, 40)), ApplyEffect, BORDER_SIZE),
                new IconSpec(Icon, new Size(AdjustSize(m_Width, 64), AdjustSize(m_Height, 64)), ApplyEffect, BORDER_SIZE),
                new IconSpec(Icon, new Size(AdjustSize(m_Width, 96), AdjustSize(m_Height, 96)), ApplyEffect, BORDER_SIZE),
                new IconSpec(Icon, new Size(AdjustSize(m_Width, 128), AdjustSize(m_Height, 128)), ApplyEffect, BORDER_SIZE),

                new IconSpec(Icon, new Size(AdjustSize(m_Width, 20), AdjustSize(m_Height, 20)), CreateMask, BORDER_SIZE),
                new IconSpec(Icon, new Size(AdjustSize(m_Width, 32), AdjustSize(m_Height, 32)), CreateMask, BORDER_SIZE),
                new IconSpec(Icon, new Size(AdjustSize(m_Width, 40), AdjustSize(m_Height, 40)), CreateMask, BORDER_SIZE),
                new IconSpec(Icon, new Size(AdjustSize(m_Width, 64), AdjustSize(m_Height, 64)), CreateMask, BORDER_SIZE),
                new IconSpec(Icon, new Size(AdjustSize(m_Width, 96), AdjustSize(m_Height, 96)), CreateMask, BORDER_SIZE),
                new IconSpec(Icon, new Size(AdjustSize(m_Width, 128), AdjustSize(m_Height, 128)), CreateMask, BORDER_SIZE)
            };
        }

        public override IIconSpec[] IconSizes { get; }

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
