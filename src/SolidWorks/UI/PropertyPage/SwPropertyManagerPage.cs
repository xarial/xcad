//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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

namespace Xarial.XCad.SolidWorks.UI.PropertyPage
{
    public interface ISwPropertyManagerPage<TModel> : IXPropertyPage<TModel>, IDisposable 
    {
    }

    /// <inheritdoc/>
    internal class SwPropertyManagerPage<TModel> : ISwPropertyManagerPage<TModel>, ISessionAttachedItem
    {
        /// <inheritdoc/>
        public event PageClosedDelegate Closed;

        /// <inheritdoc/>
        public event PageClosingDelegate Closing;

        /// <inheritdoc/>
        public event PageDataChangedDelegate DataChanged;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event Action<ISessionAttachedItem> Disposed;

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

        private readonly IServiceProvider m_SvcProvider;

        /// <summary>Creates instance of property manager page</summary>
        /// <param name="app">Pointer to session of SOLIDWORKS where the property manager page to be created</param>
        internal SwPropertyManagerPage(ISwApplication app, IServiceProvider svcProvider, SwPropertyManagerPageHandler handler,
            CreateDynamicControlsDelegate createDynCtrlHandler)
            : this(app, null, svcProvider, handler, createDynCtrlHandler)
        {
        }

        internal SwPropertyManagerPage(ISwApplication app, IPageSpec pageSpec, IServiceProvider svcProvider, SwPropertyManagerPageHandler handler,
            CreateDynamicControlsDelegate createDynCtrlHandler)
        {
            m_App = app;

            m_IsDisposed = false;

            m_SvcProvider = svcProvider;

            m_Logger = m_SvcProvider.GetService<IXLogger>();

            m_IconsConv = m_SvcProvider.GetService<IIconsCreator>();

            //TODO: validate that handler is COM visible
            Handler = handler;

            Handler.Closed += OnClosed;
            Handler.Closing += OnClosing;
            m_PmpBuilder = new PropertyManagerPageBuilder(app, m_IconsConv, Handler, pageSpec, m_Logger);

            m_Page = m_PmpBuilder.CreatePage<TModel>(createDynCtrlHandler);

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

        private void OnBindingValueChanged(IBinding binding)
            => DataChanged?.Invoke();

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

                m_IconsConv.Dispose();

                m_IsDisposed = true;

                Disposed?.Invoke(this);
            }
        }

        /// <inheritdoc/>
        public void Show(TModel model)
        {
            Model = model;
            m_Logger.Log("Opening page", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);

            const int OPTS_DEFAULT = 0;

            m_App.Sw.IActiveDoc2.ClearSelection2(true);

            foreach (var binding in m_Page.Binding.Bindings ?? Enumerable.Empty<IBinding>())
            {
                binding.Model = model;
            }

            foreach (var md in m_Page.Binding.Metadata ?? Enumerable.Empty<IMetadata>())
            {
                md.Model = model;
            }

            Handler.InvokeOpening();

            foreach (var binding in m_Page.Binding.Bindings ?? Enumerable.Empty<IBinding>())
            {
                binding.UpdateControl();
            }

            m_Page.Page.Show2(OPTS_DEFAULT);

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