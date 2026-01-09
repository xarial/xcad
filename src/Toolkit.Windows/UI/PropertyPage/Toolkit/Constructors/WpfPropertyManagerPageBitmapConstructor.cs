//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Drawing;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Constructors
{
    [DefaultType(typeof(Image))]
    internal class WpfPropertyManagerPageBitmapConstructor
        : WpfPropertyManagerPageBaseControlConstructor<WpfPropertyManagerPageBitmap>
    {
        public WpfPropertyManagerPageBitmapConstructor(IXApplication app, IIconsCreator iconsConv)
            : base(app, iconsConv)
        {
        }

        protected override WpfPropertyManagerPageBitmap Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new WpfPropertyManagerPageBitmap(parentGroup, m_IconConv, atts, metadata);
    }
}