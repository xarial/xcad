//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Constructors
{
    [DefaultType(typeof(int))]
    [DefaultType(typeof(double))]
    [DefaultType(typeof(decimal))]
    [DefaultType(typeof(float))]
    internal class WpfPropertyManagerPageNumberBoxConstructor
        : WpfPropertyManagerPageBaseControlConstructor<WpfPropertyManagerPageNumberBox>
    {
        public WpfPropertyManagerPageNumberBoxConstructor(IXApplication app, IIconsCreator iconsConv)
            : base(app, iconsConv)
        {
        }

        protected override WpfPropertyManagerPageNumberBox Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new WpfPropertyManagerPageNumberBox(parentGroup, m_IconConv, atts, metadata);
    }
}