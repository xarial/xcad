using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xarial.XCad.SolidWorks.Base;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons
{
    internal class BitmapButtonHighResIcon : BitmapButtonIcon
    {
        internal BitmapButtonHighResIcon(Image icon) : base(icon)
        {
        }

        public override IEnumerable<IconSizeInfo> GetIconSizes()
        {
            yield return new IconSizeInfo(Icon, new Size(20, 20));
            yield return new IconSizeInfo(Icon, new Size(32, 32));
            yield return new IconSizeInfo(Icon, new Size(40, 40));
            yield return new IconSizeInfo(Icon, new Size(64, 64));
            yield return new IconSizeInfo(Icon, new Size(96, 96));
            yield return new IconSizeInfo(Icon, new Size(128, 128));

            yield return new IconSizeInfo(Mask, new Size(20, 20));
            yield return new IconSizeInfo(Mask, new Size(32, 32));
            yield return new IconSizeInfo(Mask, new Size(40, 40));
            yield return new IconSizeInfo(Mask, new Size(64, 64));
            yield return new IconSizeInfo(Mask, new Size(96, 96));
            yield return new IconSizeInfo(Mask, new Size(128, 128));
        }
    }
}
