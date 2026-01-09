//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
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
    internal class WpfPropertyManagerPageOptionBoxConstructor
        : WpfPropertyManagerPageBaseControlConstructor<WpfPropertyManagerPageOptionBox>, IOptionBoxConstructor
    {
        public WpfPropertyManagerPageOptionBoxConstructor(IXApplication app, IIconsCreator iconsConv) : base(app, iconsConv)
        {
        }

        protected override WpfPropertyManagerPageOptionBox Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new WpfPropertyManagerPageOptionBox(m_App, parentGroup, m_IconConv, atts, metadata);
    }
}