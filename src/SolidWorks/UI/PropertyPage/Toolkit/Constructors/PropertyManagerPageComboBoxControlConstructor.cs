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
            IPropertyManagerPageCombobox swCtrl, IAttributeSet atts, IMetadata[] metadata, 
            SwPropertyManagerPageHandler handler, short height, IPropertyManagerPageLabel label)
        {   
            if (height != -1)
            {
                swCtrl.Height = height;
            }

            if (atts.Has<ComboBoxOptionsAttribute>())
            {
                var opts = atts.Get<ComboBoxOptionsAttribute>();

                if (opts.Style != 0)
                {
                    swCtrl.Style = (int)opts.Style;
                }
            }

            m_Helper.ParseItems(m_SwApp, atts, metadata, out bool isStatic, out ItemsControlItem[] staticItems, out IMetadata srcMetadata);

            var ctrl = new PropertyManagerPageComboBoxControl<TVal>(atts.Id, atts.Tag, swCtrl, handler, srcMetadata, label, atts.ContextType, isStatic, staticItems, metadata);

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