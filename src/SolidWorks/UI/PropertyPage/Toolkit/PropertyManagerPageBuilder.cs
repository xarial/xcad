//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xarial.XCad.Base;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.Diagnostics;
using Xarial.XCad.Utils.PageBuilder;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Binders;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit
{
    internal class PropertyManagerPageBuilder
        : PageBuilderBase<PropertyManagerPagePage, PropertyManagerPageGroupBase, IPropertyManagerPageControlEx>
    {
        private class PmpTypeDataBinder : TypeDataBinder
        {
            public PmpTypeDataBinder(IXLogger logger) : base(logger)
            {
            }

            internal event Action<IEnumerable<IBinding>> BeforeControlsDataLoad;

            internal event Func<IAttributeSet, IAttributeSet> GetPageAttributeSet;

            protected override void OnBeforeControlsDataLoad(IEnumerable<IBinding> bindings)
            {
                base.OnBeforeControlsDataLoad(bindings);

                BeforeControlsDataLoad?.Invoke(bindings);
            }

            protected override void OnGetPageAttributeSet(Type pageType, ref IAttributeSet attSet)
            {
                attSet = GetPageAttributeSet?.Invoke(attSet);
            }
        }

        private class PmpAttributeSet : IAttributeSet
        {
            private readonly IAttributeSet m_BaseAttSet;

            public Type ContextType => m_BaseAttSet.ContextType;
            public string Description => m_BaseAttSet.Description;
            public int Id => m_BaseAttSet.Id;
            public string Name { get; }
            public object Tag => m_BaseAttSet.Tag;
            public IControlDescriptor ControlDescriptor => m_BaseAttSet.ControlDescriptor;

            public void Add<TAtt>(TAtt att) where TAtt : XCad.UI.PropertyPage.Base.IAttribute
            {
                m_BaseAttSet.Add<TAtt>(att);
            }

            public TAtt Get<TAtt>() where TAtt : XCad.UI.PropertyPage.Base.IAttribute
            {
                return m_BaseAttSet.Get<TAtt>();
            }

            public IEnumerable<TAtt> GetAll<TAtt>() where TAtt : XCad.UI.PropertyPage.Base.IAttribute
            {
                return m_BaseAttSet.GetAll<TAtt>();
            }

            public bool Has<TAtt>() where TAtt : XCad.UI.PropertyPage.Base.IAttribute
            {
                return m_BaseAttSet.Has<TAtt>();
            }

            internal PmpAttributeSet(IAttributeSet baseAttSet, IPageSpec pageSpec)
            {
                m_BaseAttSet = baseAttSet;

                if (!Has<PageOptionsAttribute>())
                {
                    //TODO: process pageSpec.Icon
                    Add(new PageOptionsAttribute(pageSpec.Options));
                }

                if (string.IsNullOrEmpty(baseAttSet.Name)
                    || baseAttSet.Name == ContextType.Name)
                {
                    Name = pageSpec.Title;
                }
                else
                {
                    Name = baseAttSet.Name;
                }
            }
        }

        private readonly IPropertyManagerPageElementConstructor[] m_CtrlsContstrs;
        private readonly PmpTypeDataBinder m_DataBinder;
        private readonly IPageSpec m_PageSpec;

        internal PropertyManagerPageBuilder(SwApplication app, IIconsCreator iconsConv, SwPropertyManagerPageHandler handler, IPageSpec pageSpec, IXLogger logger)
            : this(app, new PmpTypeDataBinder(logger),
                  new PropertyManagerPageConstructor(app, iconsConv, handler),
                  new PropertyManagerPageGroupControlConstructor(app, iconsConv),
                  new PropertyManagerPageTextBoxControlConstructor(app, iconsConv),
                  new PropertyManagerPageNumberBoxConstructor(app, iconsConv),
                  new PropertyManagerPageCheckBoxControlConstructor(app, iconsConv),
                  new PropertyManagerPageEnumComboBoxControlConstructor(app, iconsConv),
                  new PropertyManagerPageCustomItemsComboBoxControlConstructor(app, iconsConv),
                  new PropertyManagerPageListBoxControlConstructor(app, iconsConv),
                  new PropertyManagerPageSelectionBoxControlConstructor(app, iconsConv),
                  new PropertyManagerPageOptionBoxConstructor(app, iconsConv),
                  new PropertyManagerPageButtonControlConstructor(app, iconsConv),
                  new PropertyManagerPageBitmapControlConstructor(app, iconsConv),
                  new PropertyManagerPageTextBlockControlConstructor(app, iconsConv),
                  new PropertyManagerPageTabConstructor(app, iconsConv),
                  new PropertyManagerPageCustomControlConstructor(app, iconsConv),
                  new PropertyManagerPageBitmapButtonConstructor(app, iconsConv))
        {
            m_PageSpec = pageSpec;
        }

        private PropertyManagerPageBuilder(ISwApplication app, PmpTypeDataBinder dataBinder, PropertyManagerPageConstructor pageConstr,
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
                return new PmpAttributeSet(attSet, m_PageSpec);
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