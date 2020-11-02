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
        /// <inheritdoc/>
        public event PageClosedDelegate Closed;

        /// <inheritdoc/>
        public event PageClosingDelegate Closing;

        /// <inheritdoc/>
        public event PageDataChangedDelegate DataChanged;

        private readonly SwApplication m_App;
        private readonly IconsConverter m_IconsConv;
        private readonly PropertyManagerPagePage m_Page;
        private readonly PropertyManagerPageBuilder m_PmpBuilder;

        /// <inheritdoc/>
        public IEnumerable<IPropertyManagerPageControlEx> Controls { get; private set; }

        internal SwPropertyManagerPageHandler Handler { get; private set; }

        public IXLogger Logger { get; }

        /// <inheritdoc/>
        public TModel Model { get; private set; }

        /// <summary>Creates instance of property manager page</summary>
        /// <param name="app">Pointer to session of SOLIDWORKS where the property manager page to be created</param>
        internal SwPropertyManagerPage(SwApplication app, IXLogger logger, Type handlerType)
            : this(app, null, logger, handlerType)
        {
        }

        internal SwPropertyManagerPage(SwApplication app, IPageSpec pageSpec, IXLogger logger, Type handlerType)
        {
            m_App = app;

            Logger = logger;

            m_IconsConv = new IconsConverter();

            //TODO: validate that handlerType inherits PropertyManagerPageHandlerEx and it is COM visible with parameterless constructor
            Handler = (SwPropertyManagerPageHandler)Activator.CreateInstance(handlerType);

            Handler.DataChanged += OnDataChanged;
            Handler.Closed += OnClosed;
            Handler.Closing += OnClosing;
            m_PmpBuilder = new PropertyManagerPageBuilder(app, m_IconsConv, Handler, pageSpec, Logger);

            m_Page = m_PmpBuilder.CreatePage<TModel>();
            Controls = m_Page.Binding.Bindings.Select(b => b.Control)
                .OfType<IPropertyManagerPageControlEx>().ToArray();
        }

        public void Dispose()
        {
            Logger.Log("Disposing page");

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
            Logger.Log("Opening page");

            const int OPTS_DEFAULT = 0;

            m_App.Sw.IActiveDoc2.ClearSelection2(true);

            foreach (var binding in m_Page.Binding.Bindings)
            {
                binding.Model = model;
            }

            m_App.ReportPropertyPageOpening(typeof(TModel));

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
        {
            Closed?.Invoke(ConvertReason(reason));
            m_App.ReportPropertyPageClosed(typeof(TModel));
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