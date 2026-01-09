//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Constructors
{
    internal class WpfPropertyManagerPageTextBlockConstructor
        : WpfPropertyManagerPageBaseControlConstructor<WpfPropertyManagerPageTextBlock>, ITextBlockConstructor
    {
        public WpfPropertyManagerPageTextBlockConstructor(IXApplication app, IIconsCreator iconConv)
            : base(app, iconConv)
        {
        }

        protected override WpfPropertyManagerPageTextBlock Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new WpfPropertyManagerPageTextBlock(parentGroup, atts, metadata, m_IconConv);
    }
}