//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Internal;

namespace Xarial.XCad.Utils.PageBuilder
{
    public class PageBuilderBase<TPage, TGroup, TControl>
        where TPage : IPage
        where TGroup : IGroup
        where TControl : IControl
    {
        private readonly IDataModelBinder m_DataBinder;
        private readonly IPageConstructor<TPage> m_PageConstructor;

        private readonly ConstructorsContainer<TPage, TGroup> m_ControlConstructors;

        public PageBuilderBase(IDataModelBinder dataBinder,
            IPageConstructor<TPage> pageConstr,
            params IPageElementConstructor<TGroup, TPage>[]
            ctrlsContstrs)
        {
            m_DataBinder = dataBinder;
            m_PageConstructor = pageConstr;

            m_ControlConstructors = new ConstructorsContainer<TPage, TGroup>(ctrlsContstrs);
        }

        public virtual TPage CreatePage<TModel>(TModel model)
        {
            var page = default(TPage);

            IEnumerable<IBinding> bindings;

            IRawDependencyGroup dependencies;

            m_DataBinder.Bind(model,
                atts =>
                {
                    page = m_PageConstructor.Create(atts);
                    return page;
                },
                (Type type, IAttributeSet atts, IGroup parent, out int idRange) =>
                {
                    idRange = 1;
                    return m_ControlConstructors.CreateElement(type, parent, atts, ref idRange);
                },
                out bindings, out dependencies);

            page.Binding.Load(bindings, dependencies);
            UpdatePageDependenciesState(page);

            return page;
        }

        protected virtual void UpdatePageDependenciesState(TPage page)
        {
            page.Binding.Dependency.UpdateAll();
        }
    }
}