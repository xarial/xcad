//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Core;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    internal abstract class PropertyManagerPageComboBoxControlConstructorBase<TVal>
        : PropertyManagerPageBaseControlConstructor<PropertyManagerPageComboBoxControl<TVal>, IPropertyManagerPageCombobox>
    {
        public PropertyManagerPageComboBoxControlConstructorBase(ISldWorks app, IconsConverter iconsConv)
            : base(app, swPropertyManagerPageControlType_e.swControlType_Combobox, iconsConv)
        {
        }

        protected override PropertyManagerPageComboBoxControl<TVal> CreateControl(
            IPropertyManagerPageCombobox swCtrl, IAttributeSet atts, SwPropertyManagerPageHandler handler, short height)
        {
            ReadOnlyCollection<TVal> itemValues;
            string[] itemNames;
            GetItems(atts, out itemNames, out itemValues);

            swCtrl.AddItems(itemNames);

            if (height != -1)
            {
                swCtrl.Height = height;
            }

            if (atts.Has<ComboBoxOptionsAttribute>())
            {
                var style = atts.Get<ComboBoxOptionsAttribute>();

                if (style.Style != 0)
                {
                    swCtrl.Style = (int)style.Style;
                }
            }

            return new PropertyManagerPageComboBoxControl<TVal>(atts.Id, atts.Tag, swCtrl, itemValues, handler);
        }

        protected abstract void GetItems(IAttributeSet atts, out string[] itemNames, out ReadOnlyCollection<TVal> itemValues);
    }

    [DefaultType(typeof(SpecialTypes.EnumType))]
    internal class PropertyManagerPageEnumComboBoxControlConstructor
        : PropertyManagerPageComboBoxControlConstructorBase<Enum>
    {
        public PropertyManagerPageEnumComboBoxControlConstructor(ISldWorks app, IconsConverter iconsConv) 
            : base(app, iconsConv)
        {
        }
        
        protected override void GetItems(IAttributeSet atts, out string[] itemNames, out ReadOnlyCollection<Enum> itemValues)
        {
            var items = EnumExtension.GetEnumFields(atts.BoundType);
            itemNames = items.Values.ToArray();
            itemValues = items.Keys.ToList().AsReadOnly();
        }
    }

    internal class PropertyManagerPageCustomItemsComboBoxControlConstructor
        : PropertyManagerPageComboBoxControlConstructorBase<object>, ICustomItemsComboBoxControlConstructor
    {
        private readonly SwApplication m_SwApp;

        public PropertyManagerPageCustomItemsComboBoxControlConstructor(SwApplication app, IconsConverter iconsConv)
            : base(app.Sw, iconsConv)
        {
            m_SwApp = app;
        }

        protected override void GetItems(IAttributeSet atts, out string[] itemNames, out ReadOnlyCollection<object> itemValues)
        {
            var customItemsAtt = atts.Get<CustomItemsAttribute>();

            var itemsProviderType = customItemsAtt.CustomItemsProviderType;

            var provider = Activator.CreateInstance(itemsProviderType) as ICustomItemsProvider;

            var items = provider.ProvideItems(m_SwApp).ToList();

            if (items == null) 
            {
                items = new List<object>();
            }

            itemNames = items.Select(i => i.ToString()).ToArray();
            itemValues = items.AsReadOnly();
        }
    }
}