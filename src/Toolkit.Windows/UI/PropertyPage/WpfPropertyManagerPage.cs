//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xarial.XCad.Base;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Toolkit.PageBuilder.Services;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Templates;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Delegates;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.PageBuilder;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage
{
    /// <summary>
    /// WPF controls-based Property Manager Page
    /// </summary>
    /// <typeparam name="TModel">Data model</typeparam>
    public interface IWpfPropertyManagerPage<TModel> : IXPropertyPage<TModel>, IDisposable 
    {
        /// <summary>
        /// Property page layout
        /// </summary>
        FrameworkElement Layout { get; }
    }
    
    /// <inheritdoc/>
    public abstract class WpfPropertyManagerPage<TModel> : IWpfPropertyManagerPage<TModel>
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

        /// <inheritdoc/>
        public TModel Model { get; private set; }

        /// <inheritdoc/>
        public IReadOnlyList<IBinding> Bindings => m_Page.Binding.Bindings;

        public bool IsPinned { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <inheritdoc/>
        public FrameworkElement Layout => m_Page.Layout;

        private readonly BaseContextProvider m_ContextProvider;
        
        private readonly WpfPropertyManagerPageBuilder m_PmpBuilder;
        private readonly WpfPropertyManagerPagePage m_Page;

        private readonly IXLogger m_Logger;

        public WpfPropertyManagerPage(IXApplication app, IServiceProvider svcProvider) 
        {
            m_Logger = svcProvider.GetService<IXLogger>();

            m_ContextProvider = new BaseContextProvider();

            var dynCtrlFactProv = new DynamicControlFactoryProvider(svcProvider);

            m_PmpBuilder = new WpfPropertyManagerPageBuilder(app, dynCtrlFactProv, svcProvider.GetService<IIconsCreator>(), svcProvider.GetService<IHelpLinkHandler>(), m_Logger);

            m_Page = m_PmpBuilder.CreatePage(this, m_ContextProvider);

            m_Page.Layout.ClosingClicked += OnClosingClicked;

            foreach (var binding in m_Page.Binding.Bindings)
            {
                binding.Changed += OnBindingValueChanged;
            }
        }

        private void OnClosingClicked(bool cancel)
        {
            Close(cancel);
        }

        public void Close(bool cancel)
        {
            ClosePage(cancel);
        }

        public void Show(TModel model)
        {
            Model = model;

            m_ContextProvider.NotifyContextChanged(model);

            foreach (var binding in m_Page.Binding.Bindings ?? Enumerable.Empty<IBinding>())
            {
                binding.UpdateControl();
            }

            ShowPage(model, m_Page.Layout, m_Page.Name);

            m_Page.Binding.Dependency.UpdateAll();
        }

        protected abstract void ShowPage(TModel model, PropertyManagerPageLayout layout, string name);

        protected abstract void ClosePage(bool cancel);

        public IDisposable Suppress()
        {
            throw new NotImplementedException();
        }

        private void OnBindingValueChanged(IBinding binding)
        {
            if (!binding.Silent)
            {
                DataChanged?.Invoke();
            }
        }

        protected bool HandleClosing(bool cancel) 
        {
            var arg = new PageClosingArg();

            Closing?.Invoke(cancel ? PageCloseReasons_e.Cancel : PageCloseReasons_e.Okay, arg);

            return !arg.Cancel;
        }

        protected void HandleClosed(bool cancel)
        {
            Closed?.Invoke(cancel ? PageCloseReasons_e.Cancel : PageCloseReasons_e.Okay);
        }

        public void Dispose()
        {
            foreach (var binding in m_Page.Binding.Bindings)
            {
                binding.Changed -= OnBindingValueChanged;

                try
                {
                    binding.Control.Dispose();
                }
                catch (Exception ex)
                {
                    m_Logger.Log(ex);
                }
            }

            m_Page.Dispose();
        }
    }
}
