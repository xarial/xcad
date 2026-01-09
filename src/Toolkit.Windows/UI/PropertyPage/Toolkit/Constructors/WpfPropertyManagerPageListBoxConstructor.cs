//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Constructors
{
    internal class WpfPropertyManagerPageListBoxConstructor
        : WpfPropertyManagerPageBaseControlConstructor<WpfPropertyManagerPageListBox>, IListBoxControlConstructor
    {
        public WpfPropertyManagerPageListBoxConstructor(IXApplication app, IIconsCreator iconsConv)
            : base(app, iconsConv)
        {
        }

        protected override WpfPropertyManagerPageListBox Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new WpfPropertyManagerPageListBox(m_App, parentGroup, m_IconConv, atts, metadata);
    }
}