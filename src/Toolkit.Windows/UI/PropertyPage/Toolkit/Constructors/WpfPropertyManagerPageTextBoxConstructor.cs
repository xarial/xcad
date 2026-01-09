//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Constructors
{
    [DefaultType(typeof(string))]
    internal class WpfPropertyManagerPageTextBoxConstructor
        : WpfPropertyManagerPageBaseControlConstructor<WpfPropertyManagerPageTextBox>
    {
        public WpfPropertyManagerPageTextBoxConstructor(IXApplication app, IIconsCreator iconConv)
            : base(app, iconConv)
        {
        }

        protected override WpfPropertyManagerPageTextBox Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new WpfPropertyManagerPageTextBox(parentGroup, atts, metadata, m_IconConv);
    }
}