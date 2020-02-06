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
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Delegates;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.Diagnostics;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage
{
    /// <inheritdoc/>
    public class SwPropertyManagerPage<TModel> : IXPropertyPage<TModel>, IDisposable
    {
        public event PageClosedDelegate Closed;

        public event PageClosingDelegate Closing;

        public event PageDataChangedDelegate DataChanged;

        private readonly ISldWorks m_App;
        private readonly IconsConverter m_IconsConv;
        private PropertyManagerPagePage m_ActivePage;
        private PropertyManagerPageBuilder m_PmpBuilder;

        /// <inheritdoc/>
        public IEnumerable<IPropertyManagerPageControlEx> Controls { get; private set; }

        /// <inheritdoc/>
        public SwPropertyManagerPageHandler Handler { get; private set; }

        public ILogger Logger { get; }

        /// <inheritdoc/>
        public TModel Model { get; private set; }

        /// <summary>Creates instance of property manager page</summary>
        /// <param name="app">Pointer to session of SOLIDWORKS where the property manager page to be created</param>
        public SwPropertyManagerPage(SwApplication app, ILogger logger, Type handlerType)
            : this(app, null, logger, handlerType)
        {
        }

        public SwPropertyManagerPage(SwApplication app, IPageSpec pageSpec, ILogger logger, Type handlerType)
        {
            m_App = app.Sw;

            Logger = logger;

            m_IconsConv = new IconsConverter();

            //TODO: validate that handlerType inherits PropertyManagerPageHandlerEx and it is COM visible with parameterless constructor
            Handler = (SwPropertyManagerPageHandler)Activator.CreateInstance(handlerType);

            Handler.DataChanged += OnDataChanged;
            Handler.Closed += OnClosed;
            Handler.Closing += OnClosing;
            m_PmpBuilder = new PropertyManagerPageBuilder(app, m_IconsConv, Handler, pageSpec, Logger);
        }

        public void Dispose()
        {
            Logger.Log("Disposing page");

            DisposeActivePage();

            m_IconsConv.Dispose();
        }

        /// <inheritdoc/>
        public void Show(TModel model)
        {
            Model = model;

            Logger.Log("Opening page");

            const int OPTS_DEFAULT = 0;

            DisposeActivePage();

            m_App.IActiveDoc2.ClearSelection2(true);

            m_ActivePage = m_PmpBuilder.CreatePage(model);
            Controls = m_ActivePage.Binding.Bindings.Select(b => b.Control)
                .OfType<IPropertyManagerPageControlEx>().ToArray();

            m_ActivePage.Page.Show2(OPTS_DEFAULT);

            //updating control states
            m_ActivePage.Binding.Dependency.UpdateAll();
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

        private void DisposeActivePage()
        {
            if (m_ActivePage != null)
            {
                foreach (var ctrl in m_ActivePage.Binding.Bindings.Select(b => b.Control).OfType<IDisposable>())
                {
                    ctrl.Dispose();
                }

                m_ActivePage = null;
            }
        }

        private void OnClosed(swPropertyManagerPageCloseReasons_e reason)
        {
            Closed?.Invoke(ConvertReason(reason));
        }

        private void OnClosing(swPropertyManagerPageCloseReasons_e reason, PageClosingArg arg)
        {
            Closing?.Invoke(ConvertReason(reason), arg);
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke();
        }
    }
}