//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    internal class PropertyManagerPageListBoxControlConstructor
        : PropertyManagerPageBaseControlConstructor<PropertyManagerPageListBoxControl, IPropertyManagerPageListbox>, IListBoxControlConstructor
    {
        private readonly ISwApplication m_SwApp;
        private readonly PropertyManagerPageItemsControlConstructorHelper m_Helper;

        public PropertyManagerPageListBoxControlConstructor(ISwApplication app, IIconsCreator iconsConv)
            : base(app.Sw, swPropertyManagerPageControlType_e.swControlType_Listbox, iconsConv)
        {
            m_SwApp = app;
            m_Helper = new PropertyManagerPageItemsControlConstructorHelper();
        }

        protected override PropertyManagerPageListBoxControl CreateControl(
            IPropertyManagerPageListbox swCtrl, IAttributeSet atts, IMetadata metadata, 
            SwPropertyManagerPageHandler handler, short height)
        {
            if (height <= 0)
            {
                height = 50;
            }

            swCtrl.Height = height;

            int style = 0;
            bool sortItems = false;

            if (atts.Has<ListBoxOptionsAttribute>())
            {
                var opts = atts.Get<ListBoxOptionsAttribute>();

                if (opts.Style != 0)
                {
                    style = (int)opts.Style;

                    if (opts.Style.HasFlag(ListBoxStyle_e.Sorted))
                    {
                        sortItems = true;
                        style -= (int)ListBoxStyle_e.Sorted;
                    }
                }
            }

            var isMultiSelect = (atts.ContextType.IsEnum
                && atts.ContextType.GetCustomAttribute<FlagsAttribute>() != null)
                || typeof(IList).IsAssignableFrom(atts.ContextType);

            if (isMultiSelect) 
            {
                style = style + (int)swPropMgrPageListBoxStyle_e.swPropMgrPageListBoxStyle_MultipleItemSelect;
            }

            swCtrl.Style = style;

            var ctrl = new PropertyManagerPageListBoxControl(atts.Id, atts.Tag, swCtrl, atts.ContextType, isMultiSelect, handler, metadata);
            var items = m_Helper.GetItems(m_SwApp, atts);
            
            if (sortItems) 
            {
                items = items.OrderBy(i => i.DisplayName).ToArray();
            }

            ctrl.Items = items;
            return ctrl;
        }
    }
}