//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using System.Drawing;
using Xarial.XCad.SolidWorks.Base;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons
{
    internal class TabIcon : IIcon
    {
        internal IXImage Icon { get; private set; }

        public Color TransparencyKey
        {
            get
            {
                return Color.White;
            }
        }

        internal TabIcon(IXImage icon)
        {
            Icon = icon;
        }

        public IEnumerable<IconSpec> GetIconSizes()
        {
            yield return new IconSpec(Icon, new Size(16, 18));
        }
    }
}