//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Constructors
{
    [DefaultType(typeof(IXSelObject))]
    [DefaultType(typeof(IEnumerable<IXSelObject>))]
    internal class WpfPropertyManagerPageSelectionBoxConstructor
        : WpfPropertyManagerPageBaseControlConstructor<WpfPropertyManagerPageSelectionBox>
    {
        private readonly IXApplication m_App;

        public WpfPropertyManagerPageSelectionBoxConstructor(IXApplication app, IIconsCreator iconsConv)
            : base(app, iconsConv)
        {
            m_App = app;
        }

        protected override WpfPropertyManagerPageSelectionBox Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new WpfPropertyManagerPageSelectionBox(parentGroup, m_IconConv, m_App, atts, metadata);
    }
}