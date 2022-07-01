//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Core;
using Xarial.XCad.Utils.PageBuilder.PageElements;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    internal abstract class PropertyManagerPageComboBoxControlConstructorBase<TVal>
        : PropertyManagerPageBaseControlConstructor<PropertyManagerPageComboBoxControl<TVal>, IPropertyManagerPageCombobox>
    {
        public PropertyManagerPageComboBoxControlConstructorBase(SwApplication app, IIconsCreator iconsConv)
            : base(app, iconsConv)
        {   
        }

        protected override PropertyManagerPageComboBoxControl<TVal> Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new PropertyManagerPageComboBoxControl<TVal>(m_App, parentGroup, m_IconConv, atts, metadata, ref numberOfUsedIds);
    }

    [DefaultType(typeof(SpecialTypes.EnumType))]
    internal class PropertyManagerPageEnumComboBoxControlConstructor
        : PropertyManagerPageComboBoxControlConstructorBase<Enum>
    {
        public PropertyManagerPageEnumComboBoxControlConstructor(SwApplication app, IIconsCreator iconsConv) 
            : base(app, iconsConv)
        {
        }
    }

    internal class PropertyManagerPageCustomItemsComboBoxControlConstructor
        : PropertyManagerPageComboBoxControlConstructorBase<object>, ICustomItemsComboBoxControlConstructor
    {
        public PropertyManagerPageCustomItemsComboBoxControlConstructor(SwApplication app, IIconsCreator iconsConv)
            : base(app, iconsConv)
        {
        }
    }
}