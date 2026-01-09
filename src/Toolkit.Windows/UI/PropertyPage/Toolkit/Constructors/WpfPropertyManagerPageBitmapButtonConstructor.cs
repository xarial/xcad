//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Constructors
{
    internal class WpfPropertyManagerPageBitmapButtonConstructor
        : WpfPropertyManagerPageBaseControlConstructor<WpfPropertyManagerPageBitmapButton>, IBitmapButtonConstructor
    {
        public WpfPropertyManagerPageBitmapButtonConstructor(IXApplication app, IIconsCreator iconsConv)
            : base(app, iconsConv)
        {
        }

        protected override WpfPropertyManagerPageBitmapButton Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new WpfPropertyManagerPageBitmapButton(parentGroup, m_IconConv, atts, metadata);
    }
}