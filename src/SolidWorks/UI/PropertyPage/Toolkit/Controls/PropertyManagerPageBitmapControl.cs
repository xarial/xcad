//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Xarial.XCad.Reflection;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal class PropertyManagerPageBitmapControl : PropertyManagerPageBaseControl<Image, IPropertyManagerPageBitmap>
    {
#pragma warning disable CS0067

        protected override event ControlValueChangedDelegate<Image> ValueChanged;

#pragma warning restore CS0067

        private readonly IIconsCreator m_IconsConv;

        private Image m_Image;
        private readonly Size m_Size;

        public PropertyManagerPageBitmapControl(IIconsCreator iconsConv,
            int id, object tag, Size? size,
            IPropertyManagerPageBitmap bitmap,
            SwPropertyManagerPageHandler handler) : base(bitmap, id, tag, handler)
        {
            m_Size = size.HasValue ? size.Value : new Size(36, 36);
            m_IconsConv = iconsConv;
        }

        protected override Image GetSpecificValue() => m_Image;

        protected override void SetSpecificValue(Image value)
        {
            IXImage img;
            if (value == null)
            {
                img = Defaults.Icon;
            }
            else 
            {
                img = new BaseImage(ImageToByteArray(value));
            }

            var icons = m_IconsConv.ConvertIcon(new ControlIcon(img, m_Size));
            SwSpecificControl.SetBitmapByName(icons[0], icons[1]);

            m_Image = value;
        }

        private byte[] ImageToByteArray(Image img)
        {
            using (var ms = new MemoryStream())
            {
                img.Save(ms, img.RawFormat);
                return ms.ToArray();
            }
        }
    }
}