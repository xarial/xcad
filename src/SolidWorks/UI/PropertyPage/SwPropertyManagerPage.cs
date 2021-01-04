//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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

namespace Xarial.XCad.SolidWorks.UI.PropertyPage
{
    public interface ISwPropertyManagerPage<TModel> : IXPropertyPage<TModel>, IDisposable 
    {
    }

    /// <inheritdoc/>
    internal class SwPropertyManagerPage<TModel> : ISwPropertyManagerPage<TModel>
    {
        /// <inheritdoc/>
        public event PageClosedDelegate Closed;

        /// <inheritdoc/>
        public event PageClosingDelegate Closing;

        /// <inheritdoc/>
        public event PageDataChangedDelegate DataChanged;

        private readonly ISwApplication m_App;
        private readonly IIconsCreator m_IconsConv;
        private readonly PropertyManagerPagePage m_Page;
        private readonly PropertyManagerPageBuilder m_PmpBuilder;

        /// <inheritdoc/>
        public IEnumerable<IPropertyManagerPageControlEx> Controls { get; private set; }

        internal SwPropertyManagerPageHandler Handler { get; private set; }

        private readonly IXLogger m_Logger;

        /// <inheritdoc/>
        public TModel Model { get; private set; }

        private readonly IServiceProvider m_SvcProvider;

        /// <summary>Creates instance of property manager page</summary>
        /// <param name="app">Pointer to session of SOLIDWORKS where the property manager page to be created</param>
        internal SwPropertyManagerPage(ISwApplication app, IServiceProvider svcProvider, Type handlerType)
            : this(app, null, svcProvider, handlerType)
        {
        }

        internal SwPropertyManagerPage(ISwApplication app, IPageSpec pageSpec, IServiceProvider svcProvider, Type handlerType)
        {
            m_App = app;

            m_SvcProvider = svcProvider;

            m_Logger = m_SvcProvider.GetService<IXLogger>();

            m_IconsConv = m_SvcProvider.GetService<IIconsCreator>();

            //TODO: validate that handlerType inherits PropertyManagerPageHandlerEx and it is COM visible with parameterless constructor
            Handler = (SwPropertyManagerPageHandler)Activator.CreateInstance(handlerType);

            Handler.DataChanged += OnDataChanged;
            Handler.Closed += OnClosed;
            Handler.Closing += OnClosing;
            m_PmpBuilder = new PropertyManagerPageBuilder(app, m_IconsConv, Handler, pageSpec, m_Logger);

            m_Page = m_PmpBuilder.CreatePage<TModel>();
            Controls = m_Page.Binding.Bindings.Select(b => b.Control)
                .OfType<IPropertyManagerPageControlEx>().ToArray();
        }

        public void Dispose()
        {
            m_Logger.Log("Disposing page");

            foreach (var ctrl in m_Page.Binding.Bindings.Select(b => b.Control).OfType<IDisposable>())
            {
                ctrl.Dispose();
            }

            m_IconsConv.Dispose();
        }

        /// <inheritdoc/>
        public void Show(TModel model)
        {
            Model = model;
            m_Logger.Log("Opening page");

            const int OPTS_DEFAULT = 0;

            m_App.Sw.IActiveDoc2.ClearSelection2(true);

            foreach (var binding in m_Page.Binding.Bindings)
            {
                binding.Model = model;
            }

            Handler.InvokeOpening();

            m_Page.Page.Show2(OPTS_DEFAULT);

            foreach (var binding in m_Page.Binding.Bindings)
            {
                binding.UpdateControl();
            }

            //updating control states
            m_Page.Binding.Dependency.UpdateAll();
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

        private void OnDataChanged() => DataChanged?.Invoke();

        public void Close(bool cancel) => m_Page.Page.Close(!cancel);
    }
}