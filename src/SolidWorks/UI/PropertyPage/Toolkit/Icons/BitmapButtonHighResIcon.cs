//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
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
        private const double DEFAULT_SIZE = 20; //default size of the button in newer versions of SOLIDWORKS

        /// <summary>
        /// Adjusting the size of the icons to match older version of SOLIDWORKS, so if add-in is updated from 2016 - the size of buttons remains the same
        /// </summary>
        internal double WidthScale { get; }
        internal double HeightScale { get; }

        internal BitmapButtonHighResIcon(IXImage icon, int width, int height, BitmapEffect_e effect = BitmapEffect_e.None)
            : base(icon, width, height, effect)
        {
            WidthScale = m_Width / DEFAULT_SIZE;
            HeightScale = m_Height / DEFAULT_SIZE;

            IconSizes = new IIconSpec[]
            {
                new IconSpec(Icon, new Size(Convert.ToInt32(WidthScale * 20d), Convert.ToInt32(HeightScale * 20d)), ApplyEffect, BORDER_SIZE),
                new IconSpec(Icon, new Size(Convert.ToInt32(WidthScale * 32d), Convert.ToInt32(HeightScale * 32d)), ApplyEffect, BORDER_SIZE),
                new IconSpec(Icon, new Size(Convert.ToInt32(WidthScale * 40d), Convert.ToInt32(HeightScale * 40d)), ApplyEffect, BORDER_SIZE),
                new IconSpec(Icon, new Size(Convert.ToInt32(WidthScale * 64d), Convert.ToInt32(HeightScale * 64d)), ApplyEffect, BORDER_SIZE),
                new IconSpec(Icon, new Size(Convert.ToInt32(WidthScale * 96d), Convert.ToInt32(HeightScale * 96d)), ApplyEffect, BORDER_SIZE),
                new IconSpec(Icon, new Size(Convert.ToInt32(WidthScale * 128d), Convert.ToInt32(HeightScale * 128d)), ApplyEffect, BORDER_SIZE),

                new IconSpec(Icon, new Size(Convert.ToInt32(WidthScale * 20d), Convert.ToInt32(HeightScale * 20d)), CreateMask, BORDER_SIZE),
                new IconSpec(Icon, new Size(Convert.ToInt32(WidthScale * 32d), Convert.ToInt32(HeightScale * 32d)), CreateMask, BORDER_SIZE),
                new IconSpec(Icon, new Size(Convert.ToInt32(WidthScale * 40d), Convert.ToInt32(HeightScale * 40d)), CreateMask, BORDER_SIZE),
                new IconSpec(Icon, new Size(Convert.ToInt32(WidthScale * 64d), Convert.ToInt32(HeightScale * 64d)), CreateMask, BORDER_SIZE),
                new IconSpec(Icon, new Size(Convert.ToInt32(WidthScale * 96d), Convert.ToInt32(HeightScale * 96d)), CreateMask, BORDER_SIZE),
                new IconSpec(Icon, new Size(Convert.ToInt32(WidthScale * 128d), Convert.ToInt32(HeightScale * 128d)), CreateMask, BORDER_SIZE)
            };
        }

        public override IIconSpec[] IconSizes { get; }
    }
}
