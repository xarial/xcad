//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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

namespace Xarial.XCad.SolidWorks.UI
{
    internal class TooltipIcon : IIcon
    {
        internal IXImage Icon { get; }

        public Color TransparencyKey => Color.White;

        public bool IsPermanent => false;

        internal TooltipIcon(IXImage icon)
        {
            Icon = icon;

            IconSizes = new IIconSpec[]
            {
                new IconSpec(Icon, new Size(16, 16))
            };
        }

        public IIconSpec[] IconSizes { get; }
    }
}
