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
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    internal abstract class PropertyManagerPageComboBoxControlConstructorBase<TVal>
        : PropertyManagerPageBaseControlConstructor<PropertyManagerPageComboBoxControl<TVal>, IPropertyManagerPageCombobox>
    {
        public PropertyManagerPageComboBoxControlConstructorBase(ISldWorks app, IIconsCreator iconsConv)
            : base(app, swPropertyManagerPageControlType_e.swControlType_Combobox, iconsConv)
        {
        }

        protected override PropertyManagerPageComboBoxControl<TVal> CreateControl(
            IPropertyManagerPageCombobox swCtrl, IAttributeSet atts, SwPropertyManagerPageHandler handler, short height)
        {   
            if (height != -1)
            {
                swCtrl.Height = height;
            }

            var selDefVal = false;

            if (atts.Has<ComboBoxOptionsAttribute>())
            {
                var opts = atts.Get<ComboBoxOptionsAttribute>();

                if (opts.Style != 0)
                {
                    swCtrl.Style = (int)opts.Style;
                }

                selDefVal = opts.SelectDefaultValue;
            }

            var ctrl = new PropertyManagerPageComboBoxControl<TVal>(atts.Id, atts.Tag, selDefVal, swCtrl, handler);
            ctrl.Items = GetItems(atts);
            return ctrl;
        }

        protected abstract ItemsControlItem[] GetItems(IAttributeSet atts);
    }

    [DefaultType(typeof(SpecialTypes.EnumType))]
    internal class PropertyManagerPageEnumComboBoxControlConstructor
        : PropertyManagerPageComboBoxControlConstructorBase<Enum>
    {
        public PropertyManagerPageEnumComboBoxControlConstructor(ISldWorks app, IIconsCreator iconsConv) 
            : base(app, iconsConv)
        {
        }
        
        protected override ItemsControlItem[] GetItems(IAttributeSet atts)
        {
            var items = EnumExtension.GetEnumFields(atts.BoundType);
            return items.Select(i => new ItemsControlItem()
            {
                DisplayName = i.Value,
                Value = i.Key
            }).ToArray();
        }
    }

    internal class PropertyManagerPageCustomItemsComboBoxControlConstructor
        : PropertyManagerPageComboBoxControlConstructorBase<object>, ICustomItemsComboBoxControlConstructor
    {
        private readonly ISwApplication m_SwApp;

        public PropertyManagerPageCustomItemsComboBoxControlConstructor(ISwApplication app, IIconsCreator iconsConv)
            : base(app.Sw, iconsConv)
        {
            m_SwApp = app;
        }

        protected override ItemsControlItem[] GetItems(IAttributeSet atts)
        {
            var customItemsAtt = atts.Get<CustomItemsAttribute>();

            var provider = customItemsAtt.CustomItemsProvider;

            var depsCount = customItemsAtt.Dependencies?.Length;

            //TODO: dependency controls cannot be loaded at this stage as binding is not yet loaded - need to sort this out
            //Not very critical at this stage as provide items wil be called as part ResolveState for dependent controls
            //For now just add a note in the documentation for this behavior
            var items = provider.ProvideItems(m_SwApp, new IControl[depsCount.Value])?.ToList();

            if (items == null) 
            {
                items = new List<object>();
            }

            return items.Select(i => new ItemsControlItem() 
            {
                DisplayName = i.ToString(), 
                Value = i 
            }).ToArray();
        }
    }
}