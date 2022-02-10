//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Drawing;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    [DefaultType(typeof(Image))]
    internal class PropertyManagerPageBitmapControlConstructor
        : PropertyManagerPageBaseControlConstructor<PropertyManagerPageBitmapControl, IPropertyManagerPageBitmap>
    {
        private readonly IIconsCreator m_IconsConv;

        public PropertyManagerPageBitmapControlConstructor(ISldWorks app, IIconsCreator iconsConv)
            : base(app, swPropertyManagerPageControlType_e.swControlType_Bitmap, iconsConv)
        {
            m_IconsConv = iconsConv;
        }

        protected override PropertyManagerPageBitmapControl CreateControl(
            IPropertyManagerPageBitmap swCtrl, IAttributeSet atts, IMetadata[] metadata, 
            SwPropertyManagerPageHandler handler, short height, IPropertyManagerPageLabel label)
        {
            Size? size = null;

            if (atts.Has<BitmapOptionsAttribute>())
            {
                var opts = atts.Get<BitmapOptionsAttribute>();
                size = opts.Size;
            }

            return new PropertyManagerPageBitmapControl(m_IconsConv, atts.Id, atts.Tag, size, swCtrl, handler, label, metadata);
        }
    }
}