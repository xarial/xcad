//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Core;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Constructors
{
    internal abstract class WpfPropertyManagerPageComboBoxConstructorBase<TVal>
        : WpfPropertyManagerPageBaseControlConstructor<WpfPropertyManagerPageComboBox<TVal>>
    {
        public WpfPropertyManagerPageComboBoxConstructorBase(IXApplication app, IIconsCreator iconConv)
            : base(app, iconConv)
        {
        }

        protected override WpfPropertyManagerPageComboBox<TVal> Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new WpfPropertyManagerPageComboBox<TVal>(m_App, parentGroup, atts, metadata, m_IconConv);
    }

    [DefaultType(typeof(SpecialTypes.EnumType))]
    internal class WpfPropertyManagerPageEnumComboBoxConstructor
        : WpfPropertyManagerPageComboBoxConstructorBase<Enum>
    {
        public WpfPropertyManagerPageEnumComboBoxConstructor(IXApplication app, IIconsCreator iconConv)
            : base(app, iconConv)
        {
        }
    }

    internal class WpfPropertyManagerPageCustomItemsComboBoxConstructor
        : WpfPropertyManagerPageComboBoxConstructorBase<object>, ICustomItemsComboBoxControlConstructor
    {
        public WpfPropertyManagerPageCustomItemsComboBoxConstructor(IXApplication app, IIconsCreator iconConv)
            : base(app, iconConv)
        {
        }
    }
}