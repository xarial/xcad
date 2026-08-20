//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.Base;
using Xarial.XCad.Toolkit.Blazor.Controls;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Delegates;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.PageBuilder;

namespace Xarial.XCad.Toolkit.Blazor
{
    /// <summary>
    /// Blazor-based property page bound to <typeparamref name="TModel"/>
    /// </summary>
    /// <remarks>
    /// <typeparam name="TModel">Data model type</typeparam>
    public class BlazorPropertyManagerPage<TModel> : IXPropertyPage<TModel>, IDisposable
    {
        /// <inheritdoc/>
        public event PageDataChangedDelegate DataChanged;

        /// <inheritdoc/>
        public event PageClosingDelegate Closing;

        /// <inheritdoc/>
        public event PageClosedDelegate Closed;

        /// <inheritdoc/>
        public event KeystrokeHookDelegate KeystrokeHook;

        /// <inheritdoc/>
        public event PagePreviewDelegate Preview;

        /// <inheritdoc/>
        public event PageUndoDelegate Undo;

        /// <inheritdoc/>
        public event PageNavigationDelegate Navigate;

        /// <summary>
        /// Raised whenever the rendered control tree changes and the hosting Razor
        /// component needs to call StateHasChanged()
        /// </summary>
        public event Action Invalidated
        {
            add => m_Page.Invalidated += value;
            remove => m_Page.Invalidated -= value;
        }

        /// <inheritdoc/>
        public TModel Model { get; private set; }

        /// <inheritdoc/>
        public IReadOnlyList<IBinding> Bindings => m_Page.Binding.Bindings;

        /// <inheritdoc/>
        public bool IsPinned { get; set; }

        /// <summary>
        /// True while the page is open (between <see cref="Show"/> and <see cref="Close"/>)
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Title of the page (from [DisplayName] on the model or its class name)
        /// </summary>
        public string Title => m_Page.Name;

        /// <summary>
        /// Message shown at the top of the page, if any
        /// </summary>
        public string Message => m_Page.Message;

        /// <summary>
        /// Buttons to render in the page footer
        /// </summary>
        public PageButtons_e Buttons => m_Page.Buttons;

        /// <summary>
        /// Root-level rendered controls
        /// </summary>
        public IReadOnlyList<IBlazorPropertyManagerPageControl> RootControls => m_Page.Controls;

        private readonly BaseContextProvider m_ContextProvider;
        private readonly BlazorPropertyManagerPageBuilder m_Builder;
        private readonly BlazorPropertyManagerPagePage m_Page;

        private readonly IXLogger m_Logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="svcProvider">Services</param>
        public BlazorPropertyManagerPage(IXApplication app, IServiceProvider svcProvider)
        {
            m_Logger = (IXLogger)svcProvider.GetService(typeof(IXLogger));

            m_ContextProvider = new BaseContextProvider();
            m_Builder = new BlazorPropertyManagerPageBuilder(app, svcProvider, m_Logger);
            m_Page = m_Builder.CreatePage(this, m_ContextProvider);

            foreach (var binding in m_Page.Binding.Bindings)
            {
                binding.Changed += OnBindingValueChanged;
            }
        }

        /// <inheritdoc/>
        public void Show(TModel model)
        {
            Model = model;

            m_ContextProvider.NotifyContextChanged(model);

            foreach (var binding in m_Page.Binding.Bindings ?? Enumerable.Empty<IBinding>())
            {
                binding.UpdateControl();
            }

            IsOpen = true;

            m_Page.Binding.Dependency.UpdateAll();

            m_Page.NotifyInvalidated();
        }

        /// <inheritdoc/>
        public void Close(bool cancel)
        {
            var arg = new PageClosingArg();

            Closing?.Invoke(cancel ? PageCloseReasons_e.Cancel : PageCloseReasons_e.Okay, arg);

            if (!arg.Cancel)
            {
                IsOpen = false;

                Closed?.Invoke(cancel ? PageCloseReasons_e.Cancel : PageCloseReasons_e.Okay);

                m_Page.NotifyInvalidated();
            }
        }

        /// <inheritdoc/>
        public IDisposable Suppress() => throw new NotImplementedException();

        private void OnBindingValueChanged(IBinding binding)
        {
            if (!binding.Silent)
            {
                DataChanged?.Invoke();
            }
        }

        public void Dispose()
        {
            m_Page.Binding.Dependency.Dispose();

            foreach (var binding in m_Page.Binding.Bindings)
            {
                binding.Changed -= OnBindingValueChanged;

                try
                {
                    binding.Dispose();
                    binding.Control.Dispose();
                }
                catch(Exception ex)
                {
                    m_Logger.Log(ex);
                }
            }
        }
    }
}
