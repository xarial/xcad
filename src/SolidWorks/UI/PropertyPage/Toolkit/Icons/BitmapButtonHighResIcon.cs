using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xarial.XCad.SolidWorks.Base;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons
{
    internal class BitmapButtonHighResIcon : BitmapButtonIcon
    {
        private const double DEFAULT_BASE_SIZE = 24; //default size of the button in older versions of SOLIDWORKS
        private const double DEFAULT_SIZE = 20; //default size of the button in newer versions of SOLIDWORKS

        /// <remarks>
        /// Adjusting the size of the icons to match older version of SOLIDWORKS, so if add-in is updated from 2016 - the size of buttons remains the same
        /// </remarks>
        internal BitmapButtonHighResIcon(Image icon, double scale) : base(icon, scale * (DEFAULT_BASE_SIZE / DEFAULT_SIZE))
        {
        }

        public override IEnumerable<IconSizeInfo> GetIconSizes()
        {
            yield return new IconSizeInfo(Icon, new Size(GetSize(20), GetSize(20)));
            yield return new IconSizeInfo(Icon, new Size(GetSize(32), GetSize(32)));
            yield return new IconSizeInfo(Icon, new Size(GetSize(40), GetSize(40)));
            yield return new IconSizeInfo(Icon, new Size(GetSize(64), GetSize(64)));
            yield return new IconSizeInfo(Icon, new Size(GetSize(96), GetSize(96)));
            yield return new IconSizeInfo(Icon, new Size(GetSize(128), GetSize(128)));

            yield return new IconSizeInfo(Mask, new Size(GetSize(20), GetSize(20)));
            yield return new IconSizeInfo(Mask, new Size(GetSize(32), GetSize(32)));
            yield return new IconSizeInfo(Mask, new Size(GetSize(40), GetSize(40)));
            yield return new IconSizeInfo(Mask, new Size(GetSize(64), GetSize(64)));
            yield return new IconSizeInfo(Mask, new Size(GetSize(96), GetSize(96)));
            yield return new IconSizeInfo(Mask, new Size(GetSize(128), GetSize(128)));
        }
    }
}
