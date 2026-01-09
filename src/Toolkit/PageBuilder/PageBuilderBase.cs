//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Delegates;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Internal;

namespace Xarial.XCad.Utils.PageBuilder
{
    /// <summary>
    /// Service which provides context for the data model
    /// </summary>
    public interface IContextProvider 
    {
        /// <summary>
        /// Fires when data context is changed
        /// </summary>
        event Action<IContextProvider, object> ContextChanged;
    }

    /// <summary>
    /// Base context provider
    /// </summary>
    public class BaseContextProvider : IContextProvider
    {
        /// <inheritdoc/>
        public event Action<IContextProvider, object> ContextChanged;

        /// <summary>
        /// Notifies when context is changed
        /// </summary>
        /// <param name="context">New context</param>
        public void NotifyContextChanged(object context)
            => ContextChanged?.Invoke(this, context);
    }

    /// <summary>
    /// Utility class to build page based on data model
    /// </summary>
    /// <typeparam name="TPage">Specific page type</typeparam>
    /// <typeparam name="TGroup">Specific group type</typeparam>
    /// <typeparam name="TControl">Specific control type</typeparam>
    public class PageBuilderBase<TPage, TGroup, TControl>
        where TPage : IPage
        where TGroup : IGroup
        where TControl : IControl
    {
        private readonly IXApplication m_App;

        private readonly IDataModelBinder m_DataBinder;
        private readonly IPageConstructor<TPage> m_PageConstructor;

        private readonly ConstructorsContainer<TPage, TGroup> m_ControlConstructors;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="dataBinder">Data binder</param>
        /// <param name="pageConstr">Constructor for the page</param>
        /// <param name="ctrlsConstrs">Constructor for controls</param>
        public PageBuilderBase(IXApplication app, IDataModelBinder dataBinder,
            IPageConstructor<TPage> pageConstr,
            params IPageElementConstructor[] ctrlsConstrs)
        {
            m_App = app;

            m_DataBinder = dataBinder;
            m_PageConstructor = pageConstr;

            m_ControlConstructors = new ConstructorsContainer<TPage, TGroup>(ctrlsConstrs);
        }

        /// <summary>
        /// Creates instance of the page from model
        /// </summary>
        /// <param name="pageCont">Page container</param>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="modelProvider">Context provider</param>
        /// <returns>Instance of the page</returns>
        public virtual TPage CreatePage<TModel>(IXPropertyPage<TModel> pageCont, IContextProvider modelProvider)
        {
            var page = default(TPage);

            m_DataBinder.Bind(pageCont,
                atts =>
                {
                    page = m_PageConstructor.Create(atts);
                    return page;
                },
                (Type type, IAttributeSet atts, IGroup parent, IMetadata[] metadata, out int numberOfUsedIds) =>
                {
                    numberOfUsedIds = 1;
                    return m_ControlConstructors.CreateElement(type, parent, atts, metadata, ref numberOfUsedIds);
                }, modelProvider,
                out IReadOnlyList<IBinding> bindings,
                out IRawDependencyGroup dependencies,
                out IMetadata[] allMetadata);

            page.Binding.Load(m_App, bindings, dependencies, allMetadata);
            UpdatePageDependenciesState(page);

            return page;
        }

        /// <summary>
        /// Updated dependencies for the controls bindings
        /// </summary>
        /// <param name="page">Parent page</param>
        protected virtual void UpdatePageDependenciesState(TPage page)
            => page.Binding.Dependency.UpdateAll();
    }
}