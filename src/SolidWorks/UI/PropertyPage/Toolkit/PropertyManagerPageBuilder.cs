//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xarial.XCad.Base;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.PageBuilder.Services;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.Utils.Diagnostics;
using Xarial.XCad.Utils.PageBuilder;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Binders;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit
{
    internal class PropertyManagerPageBuilder
        : PageBuilderBase<PropertyManagerPagePage, PropertyManagerPageGroupBase, IPropertyManagerPageControlEx>
    {
        private readonly IPropertyManagerPageElementConstructor[] m_CtrlsContstrs;
        private readonly TypeDataBinder m_DataBinder;
        private readonly IPageSpec m_PageSpec;

        internal PropertyManagerPageBuilder(SwApplication app, IIconsCreator iconsConv, IHelpLinkHandler helpLinkHandler, IDynamicControlFactoryProvider dynCtrlFactProv,
            SwPropertyManagerPageHandler handler, IPageSpec pageSpec, IXLogger logger)
            : this(app, new TypeDataBinder(dynCtrlFactProv, logger),
                  new PropertyManagerPageConstructor(app, iconsConv, helpLinkHandler, handler),
                  new PropertyManagerPageGroupControlConstructor(app, iconsConv),
                  new PropertyManagerPageTextBoxControlConstructor(app, iconsConv),
                  new PropertyManagerPageNumberBoxConstructor(app, iconsConv),
                  new PropertyManagerPageCheckBoxControlConstructor(app, iconsConv),
                  new PropertyManagerPageEnumComboBoxControlConstructor(app, iconsConv),
                  new PropertyManagerPageCustomItemsComboBoxControlConstructor(app, iconsConv),
                  new PropertyManagerPageListBoxControlConstructor(app, iconsConv),
                  new PropertyManagerPageSelectionBoxControlConstructor(app, iconsConv),
                  new PropertyManagerPageOptionBoxConstructor(app, iconsConv),
                  new PropertyManagerPageCheckBoxListConstructor(app, iconsConv),
                  new PropertyManagerPageButtonControlConstructor(app, iconsConv),
                  new PropertyManagerPageBitmapControlConstructor(app, iconsConv),
                  new PropertyManagerPageTextBlockControlConstructor(app, iconsConv),
                  new PropertyManagerPageTabConstructor(app, iconsConv),
                  new PropertyManagerPageCustomControlConstructor(app, iconsConv),
                  new PropertyManagerPageBitmapButtonConstructor(app, iconsConv))
        {
            m_PageSpec = pageSpec;
        }

        private PropertyManagerPageBuilder(ISwApplication app, TypeDataBinder dataBinder, PropertyManagerPageConstructor pageConstr,
            params IPropertyManagerPageElementConstructor[] ctrlsContstrs)
            : base(app, dataBinder, pageConstr, ctrlsContstrs)
        {
            m_DataBinder = dataBinder;
            m_CtrlsContstrs = ctrlsContstrs;

            m_DataBinder.GetPageAttributeSet += OnGetPageAttributeSet;
            m_DataBinder.BeforeControlsDataLoad += OnBeforeControlsDataLoad;
        }

        private IAttributeSet OnGetPageAttributeSet(IAttributeSet attSet)
        {
            if (m_PageSpec != null)
            {
                return m_PageSpec.ToAttributeSet(attSet);
            }

            return attSet;
        }

        private void OnBeforeControlsDataLoad(IEnumerable<IBinding> bindings)
        {
            var ctrls = bindings.Select(b => b.Control)
                .OfType<IPropertyManagerPageControlEx>().ToArray();

            foreach (var ctrlGroup in ctrls.GroupBy(c => c.GetType()))
            {
                foreach (var constr in m_CtrlsContstrs.Where(c => c.ControlType == ctrlGroup.Key))
                {
                    constr.PostProcessControls(ctrlGroup);
                }
            }
        }

        protected override void UpdatePageDependenciesState(PropertyManagerPagePage page)
        {
            //NOTE: skipping the updated before page is shown otherwise control state won't be updated correctly
            //instead updating it with UpdateAll after page is shown
        }
    }
}