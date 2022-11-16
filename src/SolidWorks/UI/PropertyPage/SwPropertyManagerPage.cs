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
using System.Linq;
using Xarial.XCad.Base;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Delegates;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.Diagnostics;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Toolkit;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.Exceptions;
using Xarial.XCad.SolidWorks.UI.Toolkit;
using System.ComponentModel;
using Xarial.XCad.Utils.PageBuilder;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage
{
    public interface ISwPropertyManagerPage<TModel> : IXPropertyPage<TModel>, IDisposable 
    {
    }

    /// <inheritdoc/>
    internal class SwPropertyManagerPage<TModel> : ISwPropertyManagerPage<TModel>, IAutoDisposable
    {
        /// <inheritdoc/>
        public event PageClosedDelegate Closed;

        /// <inheritdoc/>
        public event PageClosingDelegate Closing;

        /// <inheritdoc/>
        public event PageDataChangedDelegate DataChanged;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event Action<IAutoDisposable> Disposed;

        private readonly ISwApplication m_App;
        private readonly IIconsCreator m_IconsConv;
        private readonly PropertyManagerPagePage m_Page;
        private readonly PropertyManagerPageBuilder m_PmpBuilder;

        /// <inheritdoc/>
        public IEnumerable<IPropertyManagerPageElementEx> Controls { get; private set; }

        internal SwPropertyManagerPageHandler Handler { get; private set; }

        private readonly IXLogger m_Logger;

        private bool m_IsDisposed;

        /// <inheritdoc/>
        public TModel Model { get; private set; }

        /// <inheritdoc/>
        public bool IsPinned 
        {
            get => m_Page.Page.Pinned;
            set => m_Page.Page.Pinned = value;
        }

        private readonly IServiceProvider m_SvcProvider;

        private readonly IContextProvider m_ContextProvider;

        /// <summary>Creates instance of property manager page</summary>
        /// <param name="app">Pointer to session of SOLIDWORKS where the property manager page to be created</param>
        internal SwPropertyManagerPage(SwApplication app, IServiceProvider svcProvider, SwPropertyManagerPageHandler handler,
            CreateDynamicControlsDelegate createDynCtrlHandler)
            : this(app, null, svcProvider, handler, createDynCtrlHandler)
        {
        }

        internal SwPropertyManagerPage(SwApplication app, IPageSpec pageSpec, IServiceProvider svcProvider, SwPropertyManagerPageHandler handler,
            CreateDynamicControlsDelegate createDynCtrlHandler)
        {
            m_App = app;

            m_IsDisposed = false;

            m_SvcProvider = svcProvider;

            m_Logger = m_SvcProvider.GetService<IXLogger>();

            m_IconsConv = m_SvcProvider.GetService<IIconsCreator>();

            Handler = handler;

            ValidateHandler(Handler);            

            Handler.Closed += OnClosed;
            Handler.Closing += OnClosing;
            m_PmpBuilder = new PropertyManagerPageBuilder(app, m_IconsConv, Handler, pageSpec, m_Logger);

            m_ContextProvider = new BaseContextProvider();

            m_Page = m_PmpBuilder.CreatePage<TModel>(createDynCtrlHandler, m_ContextProvider);

            var ctrls = new List<IPropertyManagerPageElementEx>();

            foreach (var binding in m_Page.Binding.Bindings) 
            {
                binding.Changed += OnBindingValueChanged;
                
                var ctrl = binding.Control;

                if (ctrl is IPropertyManagerPageElementEx)
                {
                    ctrls.Add((IPropertyManagerPageElementEx)ctrl);
                }
                else 
                {
                    m_Logger.Log($"Unrecognized control type: {ctrl?.GetType().FullName}", XCad.Base.Enums.LoggerMessageSeverity_e.Error);
                }
            }

            Controls = ctrls.ToArray();
        }

        private void ValidateHandler(SwPropertyManagerPageHandler handler)
        {
            if (handler == null) 
            {
                throw new NullReferenceException("Handler is null");
            }

            var type = handler.GetType();

            if (!type.IsComVisible()) 
            {
                throw new Exception($"Handler type '{type.FullName}' must be COM visible");
            }

            if (!(type.IsPublic || type.IsNestedPublic)) 
            {
                throw new Exception($"Handler type '{type.FullName}' must be a public class");
            }
        }

        private void OnBindingValueChanged(IBinding binding)
        {
            if (!binding.Silent) 
            {
                DataChanged?.Invoke();
            }
        }

        public void Dispose()
        {
            if (!m_IsDisposed)
            {
                m_Logger.Log("Disposing page", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);

                foreach (var ctrl in m_Page.Binding.Bindings.Select(b => b.Control).OfType<IDisposable>())
                {
                    try
                    {
                        ctrl.Dispose();
                    }
                    catch (Exception ex)
                    {
                        m_Logger.Log(ex);
                    }
                }

                m_Page.Dispose();

                m_IsDisposed = true;

                Disposed?.Invoke(this);
            }
        }

        /// <inheritdoc/>
        public void Show(TModel model)
        {
            Model = model;
            m_Logger.Log("Opening page", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);

            m_App.Sw.IActiveDoc2.ClearSelection2(true);

            m_ContextProvider.NotifyContextChanged(model);

            Handler.InvokeOpening();

            //NOTE: controls must be updated before the page is displayed
            foreach (var binding in m_Page.Binding.Bindings ?? Enumerable.Empty<IBinding>())
            {
                binding.UpdateControl();
            }

            m_Page.Show();

            //updating control states
            m_Page.Binding.Dependency.UpdateAll();

            Handler.InvokeOpened();
        }

        private PageCloseReasons_e ConvertReason(swPropertyManagerPageCloseReasons_e reason)
        {
            switch (reason)
            {
                case swPropertyManagerPageCloseReasons_e.swPropertyManagerPageClose_Okay:
                    return PageCloseReasons_e.Okay;

                case swPropertyManagerPageCloseReasons_e.swPropertyManagerPageClose_Cancel:
                    return PageCloseReasons_e.Cancel;

                case swPropertyManagerPageCloseReasons_e.swPropertyManagerPageClose_Apply:
                    return PageCloseReasons_e.Apply;

                default:
                    return PageCloseReasons_e.Unknown;
            }
        }

        private void OnClosed(swPropertyManagerPageCloseReasons_e reason)
            => Closed?.Invoke(ConvertReason(reason));

        private void OnClosing(swPropertyManagerPageCloseReasons_e reason, PageClosingArg arg)
            => Closing?.Invoke(ConvertReason(reason), arg);

        public void Close(bool cancel) => m_Page.Page.Close(!cancel);
    }
}