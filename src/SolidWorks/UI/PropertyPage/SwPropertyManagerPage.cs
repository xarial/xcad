//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
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
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage
{
    public interface ISwPropertyManagerPage<TModel> : IXPropertyPage<TModel>, IDisposable 
    {
    }

    internal class SwPropertyManagerPageSuppressor : IDisposable
    {
        private readonly Action<SwPropertyManagerPageSuppressor> m_DisposedHandler;

        internal SwPropertyManagerPageSuppressor(Action<SwPropertyManagerPageSuppressor> dispHandler) 
        {
            m_DisposedHandler = dispHandler;
        }

        public void Dispose()
        {
            m_DisposedHandler?.Invoke(this);
        }
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

        /// <inheritdoc/>
        public event KeystrokeHookDelegate KeystrokeHook;

        /// <inheritdoc/>
        public event PagePreviewDelegate Preview;

        /// <inheritdoc/>
        public event PageUndoDelegate Undo;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event Action<IAutoDisposable> Disposed;

        private readonly ISwApplication m_App;
        private readonly IIconsCreator m_IconsConv;
        private readonly PropertyManagerPagePage m_Page;
        private readonly PropertyManagerPageBuilder m_PmpBuilder;

        internal SwPropertyManagerPageHandler Handler { get; }

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

        private readonly IReadOnlyDictionary<int, IControl> m_Controls;

        private bool m_IsPreviewEnabled;

        private bool m_IsShown;

        private bool m_IsSuppressed;

        private bool m_IsClosing;

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

            Handler.Keystroke += OnKeystroke;
            Handler.Preview += OnPreview;
            Handler.Undo += OnUndo;
            Handler.Redo += OnRedo;
            Handler.Closed += OnClosed;
            Handler.Closing += OnClosing;
            m_PmpBuilder = new PropertyManagerPageBuilder(app, m_IconsConv, Handler, pageSpec, m_Logger);

            m_ContextProvider = new BaseContextProvider();

            m_Page = m_PmpBuilder.CreatePage<TModel>(createDynCtrlHandler, m_ContextProvider);

            var ctrls = new Dictionary<int, IControl>();

            foreach (var binding in m_Page.Binding.Bindings) 
            {
                binding.Changed += OnBindingValueChanged;
                
                var ctrl = binding.Control;

                ctrls.Add(ctrl.Id, ctrl);
            }

            m_Controls = ctrls;

            m_IsShown = false;
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

                Handler.Keystroke -= OnKeystroke;
                Handler.Preview -= OnPreview;
                Handler.Undo -= OnUndo;
                Handler.Redo -= OnRedo;
                Handler.Closed -= OnClosed;
                Handler.Closing -= OnClosing;

                m_IsDisposed = true;

                Disposed?.Invoke(this);
            }
        }

        /// <inheritdoc/>
        public void Show(TModel model)
        {
            m_IsClosing = false;

            Model = model;
            m_Logger.Log("Opening page", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);

            var activeDoc = m_App.Sw.IActiveDoc2;

            if (activeDoc != null)
            {
                activeDoc.ClearSelection2(true);

                m_ContextProvider.NotifyContextChanged(model);

                Handler.InvokeOpening();

                //NOTE: controls must be updated before the page is displayed
                foreach (var binding in m_Page.Binding.Bindings ?? Enumerable.Empty<IBinding>())
                {
                    binding.UpdateControl();
                }

                m_Page.Show();

                m_IsShown = true;

                //updating control states
                m_Page.Binding.Dependency.UpdateAll();

                Handler.InvokeOpened();
            }
            else 
            {
                throw new NotSupportedException("Property Manager Page can only be show in the visible document");
            }
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
        {
            m_IsShown = false;

            if (!m_IsSuppressed)
            {
                if (m_Page.IsRestorable && (reason == swPropertyManagerPageCloseReasons_e.swPropertyManagerPageClose_UnknownReason || !m_IsClosing))
                {
                    //reopen page if system closed and page is restorable
                    //when system closes page (e.g. not the user clicks, but some other command may hide page) either swPropertyManagerPageClose_UnknownReason reason is provided or OnClose event is not raised
                    Show(Model);
                }
                else 
                {
                    Closed?.Invoke(ConvertReason(reason));
                }
            }
        }

        private void OnClosing(swPropertyManagerPageCloseReasons_e reason, PageClosingArg arg)
        {
            if (!m_IsSuppressed)
            {
                //do not close the page if it is closed by the system and page is restorable
                if (!m_Page.IsRestorable || reason != swPropertyManagerPageCloseReasons_e.swPropertyManagerPageClose_UnknownReason)
                {
                    Closing?.Invoke(ConvertReason(reason), arg);
                    m_IsClosing = !arg.Cancel;
                }
            }
        }

        private bool OnKeystroke(int wParam, int message, int lParam, int id)
        {
            var handled = false;

            m_Controls.TryGetValue(id, out var ctrl);

            KeystrokeHook?.Invoke(ctrl, message, new IntPtr(wParam), new IntPtr(lParam), ref handled);

            return handled;
        }

        private void OnRedo() => Undo?.Invoke(PageUndoRedoAction_e.Redo);

        private void OnUndo() => Undo?.Invoke(PageUndoRedoAction_e.Undo);

        private bool OnPreview()
        {
            var cancel = false;

            var newPreviewEnabled = !m_IsPreviewEnabled;

            Preview?.Invoke(newPreviewEnabled, ref cancel);

            if (!cancel) 
            {
                m_IsPreviewEnabled = newPreviewEnabled;
            }

            return !cancel;
        }

        public void Close(bool cancel) => m_Page.Page.Close(!cancel);

        public IDisposable Suppress()
        {
            if (m_IsShown)
            {
                if (!m_IsSuppressed)
                {
                    m_IsSuppressed = true;
                    Close(false);
                    return new SwPropertyManagerPageSuppressor(Unsuppress);
                }
                else 
                {
                    throw new Exception("Page is already suppressed");
                }
            }
            else 
            {
                throw new Exception("Page is not shown and cannot be suppressed");
            }
        }

        private void Unsuppress(SwPropertyManagerPageSuppressor obj)
        {
            if (m_IsSuppressed)
            {
                m_IsSuppressed = false;

                if (!m_IsShown)
                {
                    Show(Model);
                }
                else
                {
                    throw new Exception("Page is shown and cannot be unsuppressed");
                }
            }
            else 
            {
                throw new Exception("Page is not suppressed");
            }
        }
    }
}