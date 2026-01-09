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
    internal class WpfPropertyManagerPageCustomControlConstructor
        : WpfPropertyManagerPageBaseControlConstructor<WpfPropertyManagerPageCustomControl>, ICustomControlConstructor
    {
        public WpfPropertyManagerPageCustomControlConstructor(IXApplication app, IIconsCreator iconsConv)
            : base(app, iconsConv)
        {
        }

        protected override WpfPropertyManagerPageCustomControl Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new WpfPropertyManagerPageCustomControl(parentGroup, m_IconConv, atts, metadata);
    }
}