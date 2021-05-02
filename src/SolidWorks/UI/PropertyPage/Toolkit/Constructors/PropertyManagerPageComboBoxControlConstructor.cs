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
        protected readonly ISwApplication m_SwApp;

        private readonly PropertyManagerPageItemsControlConstructorHelper m_Helper;

        public PropertyManagerPageComboBoxControlConstructorBase(ISwApplication app, IIconsCreator iconsConv)
            : base(app.Sw, swPropertyManagerPageControlType_e.swControlType_Combobox, iconsConv)
        {
            m_SwApp = app;
            m_Helper = new PropertyManagerPageItemsControlConstructorHelper();
        }

        protected override PropertyManagerPageComboBoxControl<TVal> CreateControl(
            IPropertyManagerPageCombobox swCtrl, IAttributeSet atts, IMetadata metadata, 
            SwPropertyManagerPageHandler handler, short height)
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

            var ctrl = new PropertyManagerPageComboBoxControl<TVal>(atts.Id, atts.Tag, selDefVal, swCtrl, handler, metadata);
            ctrl.Items = m_Helper.GetItems(m_SwApp, atts);
            return ctrl;
        }
    }

    [DefaultType(typeof(SpecialTypes.EnumType))]
    internal class PropertyManagerPageEnumComboBoxControlConstructor
        : PropertyManagerPageComboBoxControlConstructorBase<Enum>
    {
        public PropertyManagerPageEnumComboBoxControlConstructor(ISwApplication app, IIconsCreator iconsConv) 
            : base(app, iconsConv)
        {
        }
    }

    internal class PropertyManagerPageCustomItemsComboBoxControlConstructor
        : PropertyManagerPageComboBoxControlConstructorBase<object>, ICustomItemsComboBoxControlConstructor
    {
        public PropertyManagerPageCustomItemsComboBoxControlConstructor(ISwApplication app, IIconsCreator iconsConv)
            : base(app, iconsConv)
        {
        }
    }
}