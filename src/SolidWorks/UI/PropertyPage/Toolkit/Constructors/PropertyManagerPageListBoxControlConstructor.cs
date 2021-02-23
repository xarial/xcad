//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Collections;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
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
            if (height != -1)
            {
                swCtrl.Height = height;
            }

            //var selDefVal = false;

            //if (atts.Has<ComboBoxOptionsAttribute>())
            //{
            //    var opts = atts.Get<ComboBoxOptionsAttribute>();

            //    if (opts.Style != 0)
            //    {
            //        swCtrl.Style = (int)opts.Style;
            //    }

            //    selDefVal = opts.SelectDefaultValue;
            //}

            var ctrl = new PropertyManagerPageListBoxControl(atts.Id, atts.Tag, swCtrl, handler);
            ctrl.Items = m_Helper.GetItems(m_SwApp, atts);
            return ctrl;
        }
    }
}