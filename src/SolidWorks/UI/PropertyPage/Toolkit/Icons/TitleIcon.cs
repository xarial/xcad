//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using System.Drawing;
using Xarial.XCad.SolidWorks.Base;
using Xarial.XCad.Toolkit.Base;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons
{
    internal class TitleIcon : IIcon
    {
        internal IXImage Icon { get; }

        public Color TransparencyKey => Color.White;

        public bool IsPermanent => false;

        public IconImageFormat_e Format => IconImageFormat_e.Bmp;

        internal TitleIcon(IXImage icon)
        {
            Icon = icon;
            IconSizes = new IIconSpec[]
            {
                new IconSpec(Icon, new Size(22, 22))
            };
        }

        public IIconSpec[] IconSizes { get; }
    }
}