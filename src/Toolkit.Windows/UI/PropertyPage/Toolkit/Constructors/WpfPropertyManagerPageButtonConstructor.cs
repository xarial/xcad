//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Constructors
{
    [DefaultType(typeof(Action))]
    internal class WpfPropertyManagerPageButtonConstructor
        : WpfPropertyManagerPageBaseControlConstructor<WpfPropertyManagerPageButton>
    {
        public WpfPropertyManagerPageButtonConstructor(IXApplication app, IIconsCreator iconConv)
            : base(app, iconConv)
        {
        }

        protected override WpfPropertyManagerPageButton Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new WpfPropertyManagerPageButton(parentGroup, atts, metadata, m_IconConv);
    }
}