//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Xarial.XCad.Reflection;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal class PropertyManagerPageBitmapControl : PropertyManagerPageBaseControl<Image, IPropertyManagerPageBitmap>
    {
#pragma warning disable CS0067

        protected override event ControlValueChangedDelegate<Image> ValueChanged;

#pragma warning restore CS0067

        private Image m_Image;
        private Size m_Size;

        private IImageCollection m_Bitmap;

        public PropertyManagerPageBitmapControl(SwApplication app, IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            : base(app, parentGroup, iconConv, atts, metadata, swPropertyManagerPageControlType_e.swControlType_Bitmap, ref numberOfUsedIds)
        {
        }

        protected override void InitData(IControlOptionsAttribute opts, IAttributeSet atts)
        {
            Size? size = null;

            if (atts.Has<BitmapOptionsAttribute>())
            {
                var bmpOpts = atts.Get<BitmapOptionsAttribute>();
                size = bmpOpts.Size;
            }

            m_Size = size.HasValue ? size.Value : new Size(36, 36);
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
                img = new XDrawingImage(value);
            }

            m_Bitmap?.Dispose();

            m_Bitmap = m_IconConv.ConvertIcon(new ControlIcon(img, m_Size));
            SwSpecificControl.SetBitmapByName(m_Bitmap.FilePaths[0], m_Bitmap.FilePaths[1]);

            m_Image = value;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) 
            {
                base.Dispose(disposing);

                m_Bitmap?.Dispose();
            }
        }
    }
}